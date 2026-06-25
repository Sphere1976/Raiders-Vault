# Raiders Vault GraphQL BFF

This Spring GraphQL backend-for-frontend demonstrates another common enterprise integration pattern:
a purpose-built API facade for React, mobile, and dashboard clients.

## Run

```bash
mvn spring-boot:run
```

GraphiQL:

```text
http://127.0.0.1:8090/graphiql
```

Sample query:

```graphql
query GlobalOps {
  globalOpsSummary {
    generatedAt
    activeConditionCount
    newsUpdateCount
    executiveSignals
  }
}
```
