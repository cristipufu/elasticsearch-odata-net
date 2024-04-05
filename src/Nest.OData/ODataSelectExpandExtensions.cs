#if USE_ODATA_V7
using Microsoft.AspNet.OData.Query;
#else
using Microsoft.AspNetCore.OData.Query;
#endif
using Microsoft.OData.UriParser;

namespace Nest.OData
{
    public static class ODataSelectExpandExtensions
    {
        public static SearchDescriptor<T> SelectExpand<T>(this SearchDescriptor<T> searchDescriptor, SelectExpandQueryOption selectExpandQueryOption) where T : class
        {
            if (selectExpandQueryOption?.SelectExpandClause == null)
            {
                return searchDescriptor;
            }

            var selectedFields = selectExpandQueryOption.SelectExpandClause.SelectedItems
                .OfType<PathSelectItem>()
                .Select(i => i.SelectedPath.FirstSegment.Identifier)
                .ToList();

            var expands = selectExpandQueryOption.SelectExpandClause.SelectedItems
                .OfType<ExpandedNavigationSelectItem>()
                .ToList();

            var queries = new List<QueryContainer>();

            foreach (var expand in expands)
            {
                var navigationPropertyName = expand.PathToNavigationProperty.FirstSegment.Identifier;

                if (expand.FilterOption != null)
                {
                    var queryContainer = ODataFilterExtensions.TranslateExpression(expand.FilterOption.Expression, new ODataExpressionContext
                    {
                        PathPrefix = navigationPropertyName,
                        Type = typeof(T),
                    });

                    queries.Add(new NestedQuery
                    {
                        Path = navigationPropertyName,
                        Query = queryContainer,
                    });
                }
                else
                {
                    var nestedSelects = expand.SelectAndExpand.SelectedItems
                        .OfType<PathSelectItem>()
                        .Select(i => $"{navigationPropertyName}.{i.SelectedPath.FirstSegment.Identifier}")
                        .ToList();

                    selectedFields.AddRange(nestedSelects);
                }
            }

            if (selectedFields.Count > 0)
            {
                searchDescriptor = searchDescriptor.Source(s => s.Includes(i => i.Fields(selectedFields.ToArray())));
            }

            if (queries.Count == 0)
            {
                return searchDescriptor;
                
            }
            if (queries.Count == 1)
            {
                return searchDescriptor.Query(q => queries[0]);
            }

            return searchDescriptor.Query(q => new BoolQuery { Must = queries });
        }
    }
}
