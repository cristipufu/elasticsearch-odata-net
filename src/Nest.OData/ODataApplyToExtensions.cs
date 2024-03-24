using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser.Aggregation;

#nullable disable
namespace Nest.OData
{
    // todo check for complex properties or navigation properties

    public static class ODataApplyToExtensions
    {
        public static SearchDescriptor<T> ApplyTransformations<T>(this SearchDescriptor<T> searchDescriptor, ApplyQueryOption applyQueryOption) where T : class
        {
            if (applyQueryOption != null && applyQueryOption.ApplyClause != null)
            {
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
            }

            return searchDescriptor;
        }

        private static SearchDescriptor<T> ApplyGroupBy<T>(this SearchDescriptor<T> searchDescriptor, GroupByTransformationNode groupByTransformationNode) where T : class
        {
            var groupByProperties = groupByTransformationNode.GroupingProperties;

            foreach (var property in groupByProperties)
            {
                var propertyName = property.Name;

                searchDescriptor.Aggregations(a =>
                    a.Terms("group_by_" + propertyName, t => t.Field(propertyName)));
            }

            return searchDescriptor;
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
