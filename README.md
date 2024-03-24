# Elasticsearch OData Extension
This project bridges the gap between the flexibility of OData queries and the powerful search capabilities of Elasticsearch, allowing you to leverage OData query syntax to query your Elasticsearch indices. Whether you're dealing with complex nested objects, arrays, or just need to perform simple searches, this extension has got you covered.

## Getting Started
To start using this extension, include it in your project and configure it to point to your Elasticsearch instance. Here's a quick example:

```csharp
// Assuming you have an ODataQueryOptions instance from an ASP.NET Core controller
var queryOptions = ...; 

// Convert OData query options to an Elasticsearch query container
var queryContainer = queryOptions.ToQueryContainer<Document>();

// Execute the query against your Elasticsearch index
var searchResponse = await elasticClient
        .SearchAsync<Document>(s => s.Query(q => queryContainer)
);
```
Replace `Document` with your document class that maps to your Elasticsearch index.

## Features
This extension supports a wide range of OData query functionalities, tailored specifically for Elasticsearch's query DSL. Here's what you can do:

### Supported OData Logical Operators
- **`Equals`** (eq)
- **`Not Equals`** (ne)
- **`Greater Than`** (gt)
- **`Greater Than or Equal`** (ge)
- **`Less Than`** (lt)
- **`Less Than or Equal`** (le)
- **`And`**
- **`Or`**
- **`In`**

### Supported OData Functions
- **`startswith`**
- **`endswith`**
- **`contains`**
- **`substringof`**

### Supported Lambda Operators
- **`any`**
- **`all`**

### Handling Enums and Collections
Enums are treated as strings, allowing for straightforward comparisons without additional conversion steps. Collections, including simple arrays and nested objects, can be queried using the any and all functions, providing a seamless experience for working with complex data structures.

### Advanced Query Scenarios
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
## Contributing
Contributions are welcome! Whether you're fixing a bug, adding a new feature, or improving the documentation, please feel free to make a pull request.

## License
This project is licensed under the MIT License.

## Support
If you encounter any issues or have questions, please file an issue on this repository.
