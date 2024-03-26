using Microsoft.AspNetCore.OData.Query;
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

            // $select 
            var selectedFields = selectExpandQueryOption.SelectExpandClause.SelectedItems
                .OfType<PathSelectItem>()
                .Select(i => i.SelectedPath.FirstSegment.Identifier)
                .ToList();

            // $expand
            var expands = selectExpandQueryOption.SelectExpandClause.SelectedItems
                .OfType<ExpandedNavigationSelectItem>()
                .ToList();

            foreach (var expand in expands)
            {
                var navigationPropertyName = expand.PathToNavigationProperty.FirstSegment.Identifier;

                if (expand.FilterOption != null)
                {
                    var queryContainer = ODataFilterExtensions.TranslateExpression(expand.FilterOption.Expression, new ODataExpressionContext
                    {
                        PathPrefix = navigationPropertyName,
                    });

                    searchDescriptor.Query(q => new NestedQuery
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

            return searchDescriptor;
        }
    }
}
