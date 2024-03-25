using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser.Aggregation;

#nullable disable
namespace Nest.OData
{
    /// <summary>
    /// https://docs.oasis-open.org/odata/odata-data-aggregation-ext/v4.0/cs01/odata-data-aggregation-ext-v4.0-cs01.html
    /// </summary>
    public static class ODataAggregationsExtensions
    {
        public static SearchDescriptor<T> Aggregate<T>(this SearchDescriptor<T> searchDescriptor, ApplyQueryOption applyQueryOption) where T : class
        {
            if (applyQueryOption == null || applyQueryOption.ApplyClause == null)
            {
                return searchDescriptor;
            }

            foreach (var transformationNode in applyQueryOption.ApplyClause.Transformations)
            {
                searchDescriptor = transformationNode.Kind switch
                {
                    TransformationNodeKind.GroupBy => searchDescriptor.ApplyGroupBy(transformationNode as GroupByTransformationNode),
                    TransformationNodeKind.Aggregate => searchDescriptor.ApplyAggregate(transformationNode as AggregateTransformationNode),
                    TransformationNodeKind.Filter => searchDescriptor.Query(q => ODataFilterExtensions.TranslateExpression((transformationNode as FilterTransformationNode).FilterClause.Expression)),
                    _ => searchDescriptor
                };
            }

            return searchDescriptor;
        }

        private static SearchDescriptor<T> ApplyGroupBy<T>(this SearchDescriptor<T> searchDescriptor, GroupByTransformationNode groupByTransformationNode) where T : class
        {
            if (groupByTransformationNode?.GroupingProperties == null || !groupByTransformationNode.GroupingProperties.Any())
            {
                return searchDescriptor;
            }

            var groupByProperties = groupByTransformationNode.GroupingProperties.ToList();

            AggregationContainerDescriptor<T> aggregations = null;

            groupByProperties.Reverse();

            foreach (var property in groupByProperties)
            {
                var propertyName = property.Name;
                aggregations = new AggregationContainerDescriptor<T>().Terms(
                        "group_by_" + propertyName,
                        t => t.Field(propertyName).Aggregations(a => aggregations)
                    );
            }

            if (aggregations == null)
            {
                return searchDescriptor;
            }

            return searchDescriptor.Aggregations(a => aggregations);
        }

        private static SearchDescriptor<T> ApplyAggregate<T>(this SearchDescriptor<T> searchDescriptor, AggregateTransformationNode aggregateTransformationNode) where T : class
        {
            var aggregateExpressions = aggregateTransformationNode.AggregateExpressions;

            foreach (var aggregateExpression in aggregateExpressions.OfType<AggregateExpression>())
            {
                var alias = aggregateExpression.Alias;
                var propertyName = aggregateExpression.Expression.ToString();

                _ = aggregateExpression.Method switch
                {
                    AggregationMethod.Max => searchDescriptor.Aggregations(a => a.Max(alias, m => m.Field(propertyName))),
                    AggregationMethod.Min => searchDescriptor.Aggregations(a => a.Min(alias, s => s.Field(propertyName))),
                    AggregationMethod.Average => searchDescriptor.Aggregations(a => a.Average(alias, avg => avg.Field(propertyName))),
                    AggregationMethod.Sum => searchDescriptor.Aggregations(a => a.Sum(alias, s => s.Field(propertyName))),
                    AggregationMethod.CountDistinct => searchDescriptor.Aggregations(a => a.Cardinality(alias, c => c.Field(propertyName))),
                    AggregationMethod.VirtualPropertyCount => searchDescriptor.Aggregations(a => a.ValueCount(alias, vc => vc.Field(propertyName))),
                    _ => throw new NotImplementedException($"Unsupported aggregation method: {aggregateExpression.Method}")
                };
            }

            return searchDescriptor;
        }
    }
}
