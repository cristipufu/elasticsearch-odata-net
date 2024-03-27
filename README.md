# elasticsearch-odata-net

[![NuGet](https://img.shields.io/nuget/v/Nest.OData)](https://www.nuget.org/packages/Nest.OData)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=cristipufu_elasticsearch-odata-net&metric=coverage)](https://sonarcloud.io/summary/new_code?id=cristipufu_elasticsearch-odata-net)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=cristipufu_elasticsearch-odata-net&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=cristipufu_elasticsearch-odata-net)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=cristipufu_elasticsearch-odata-net&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=cristipufu_elasticsearch-odata-net)
[![GitHub](https://img.shields.io/github/license/cristipufu/elasticsearch-odata-net)](https://github.com/cristipufu/elasticsearch-odata-net/blob/master/LICENSE)

This project bridges the gap between the flexibility of OData queries and the powerful search capabilities of Elasticsearch, allowing you to leverage OData query syntax to query your Elasticsearch indices. Whether you're dealing with complex nested objects, arrays, or just need to perform simple searches, this extension has got you covered.

# install
To start using this extension, include it in your project and configure it to point to your Elasticsearch instance. Here's a quick example:

```xml
PM> Install-Package Nest.OData
```
```
TargetFramework: net8.0

Dependencies:
Microsoft.AspNetCore.OData (>= 8.2.5)
NEST (>= 7.17.5)
```

```csharp
[HttpGet]
public async Task<IActionResult> Get(ODataQueryOptions<Document> queryOptions)
{
    var searchDescriptor = queryOptions.ToElasticQuery<Document>();

    var response = await _elasticClient.SearchAsync<Document>(searchDescriptor);

    if (response.IsValid)
    {
        return Ok(response.Documents);
    }
    else
    {
        return BadRequest();
    }
}
```
Replace `Document` with your document class that maps to your Elasticsearch index.

# features
This extension supports a wide range of OData query functionalities, tailored specifically for Elasticsearch's query DSL. Here's what you can do:

## Supported Features

- **Filtering** (`$filter`): Translate OData filters into Elasticsearch query DSL, supporting logical operators, comparison operations, and some basic functions.
- **Ordering** (`$orderby`): Support for sorting by multiple fields, including support for nested objects.
- **Pagination** (`$skip` and `$top`): Implement pagination through Elasticsearch's `from` and `size` parameters.
- **Aggregation** (`$apply`): Support for translating aggregation transformations, including `groupby` and aggregate functions like `sum`, `max`, `min`, `average`, and `countdistinct`.
- **Selection** (`$select`): Ability to specify which fields to include in the response, reducing the payload size and focusing on the relevant data. 
- **Expansion** (`$expand`): Support for applying additional `$filter` and `$select` conditions on complex nested objects.

## Supported OData Logical Operators
- **`Equals`** (eq)
- **`Not Equals`** (ne)
- **`Greater Than`** (gt)
- **`Greater Than or Equal`** (ge)
- **`Less Than`** (lt)
- **`Less Than or Equal`** (le)
- **`And`**
- **`Or`**
- **`In`**

## Supported OData Functions
- **`startswith`**
- **`endswith`**
- **`contains`**
- **`substringof`**

## Supported Lambda Operators
- **`any`**
- **`all`**

## Handling Enums and Collections
Enums are treated as strings, allowing for straightforward comparisons without additional conversion steps. Collections, including simple arrays and nested objects, can be queried using the any and all functions, providing a seamless experience for working with complex data structures.

## Advanced Query Scenarios
The extension provides support for nested queries, allowing you to delve into nested objects and arrays within your documents to perform fine-grained searches. Whether you're filtering on properties of nested objects or querying arrays for specific elements, this extension translates your OData queries into efficient Elasticsearch DSL queries.

`$filter=Tags/any(t: t/Name eq 'bug')`
```json

{
    "query": {
        "nested": {
            "path": "Tags",
            "query": {
              "term": {
                  "Name": {
                    "value": "bug"
                  }
              }
          }
      }
  }
}
```
`$filter=Category in ('Electronics', 'Books')`
```json
{
  "query": {
    "terms": {
      "Category": ["Electronics", "Books"]
    }
  }
}
```
`$filter=Id eq 42 and ((Color eq 'Red') or (Color eq 'Green') or (Color eq 'Blue'))`
```json
{
  "query": {
    "bool": {
      "must": [
        {
          "term": {
            "Id": {
              "value": 42
            }
          }
        },
        {
          "bool": {
            "minimum_should_match": 1,
            "should": [
              {
                "term": {
                  "Color": {
                    "value": "Red"
                  }
                }
              },
              {
                "term": {
                  "Color": {
                    "value": "Green"
                  }
                }
              },
              {
                "term": {
                  "Color": {
                    "value": "Blue"
                  }
                }
              }
            ]
          }
        }
      ]
    }
  }
}
```
# contributing
Contributions are welcome! Whether you're fixing a bug, adding a new feature, or improving the documentation, please feel free to make a pull request.

# license
This project is licensed under the MIT License.

# support
If you encounter any issues or have questions, please file an issue on this repository.
