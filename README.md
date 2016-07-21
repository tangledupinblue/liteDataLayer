# liteDataLayer

Simple data layer for dotnet core

Not yet complete, please note there may be breaking changes in the short term
Breaking changes will mainly be around the scripting

Please forgive testing that is built into the project

##3 main components:
DataAccess Layer - executing sql; 
    - liteDataTable - very simple tabular data structure as DataTable and DataSet not implemented in dotnet core
    - wrappers for execute nonquery, execute scalar

Scripting - Scripting of sql based on directives with a simple syntax
    - Formatting to sql for insert, update, delete etc

LiteOrm - lite Orm mapper
    - Not bound to actual database schema
        - Better for older databases
        - Easier to implement backward compatibility with databases 
            - In other words should be possible to update database then application for continuous integration
    - Schema inferred from convention and then overridden using "directives"


