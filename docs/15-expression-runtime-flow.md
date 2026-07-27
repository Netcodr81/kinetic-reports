# 15 - Expression Runtime Flow

This document breaks down how expression evaluation executes in the core engine pipeline, regardless of which host application invokes report generation.

## Expression Sub-Pipeline

```mermaid
sequenceDiagram
    participant Builder as DefaultReportBuilder
    participant Evaluator as DefaultExpressionEvaluator IExpressionEvaluator
    participant Runtime as FunctionExpressionRuntime
    participant Parser as Parser

    Builder->>Evaluator: 1 EvaluateAsString(expression string) or Evaluate(expression string)
    Evaluator->>Evaluator: 2 Evaluate(expression string, context ExpressionContext)
    alt 3 Function style expression
        Evaluator->>Runtime: 4 Evaluate(expression string, context ExpressionContext)
        Runtime->>Parser: 5 ParseExpression()
        Parser-->>Runtime: 6 Node tree
        Runtime->>Runtime: 7 EvaluateNode(node Node, context ExpressionContext)
        Runtime->>Runtime: 8 EvaluateFunction(call FunctionCallNode, context ExpressionContext)
        Runtime->>Runtime: 9 Dispatch to function handlers
        Runtime-->>Evaluator: 10 object result
    else 3 Token expression or mixed literal
        Evaluator->>Evaluator: 4 ResolveToken(tokenName string, context ExpressionContext)
        Evaluator->>Evaluator: 5 TokenPattern Replace with ResolveToken
        Evaluator-->>Builder: 6 object result
    end
    Builder->>Builder: 11 ToText(value object)
```

## Expression Evaluation Walkthrough

1. DefaultReportBuilder asks IExpressionEvaluator for a value.
2. DefaultExpressionEvaluator receives the expression string and context.
3. It decides whether the input is a function-style expression.
4. If function-style, FunctionExpressionRuntime parses and executes the expression tree.
5. Function execution routes to math, string, date, conditional, collection, aggregate, formatting, or report handlers.
6. If not function-style, token resolution is used for direct token values or token replacement in text.
7. The evaluated object result returns to the builder.
8. Builder converts the value to rendering text when needed.

## Methods Involved

| Component                            | Methods involved                                                                                                                                                                                                                                      |
| ------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `DefaultReportBuilder`             | `EvaluateAsString(evaluator: IExpressionEvaluator, expression: string, context: ExpressionContext)`, `RowBuildContext.Evaluate(expression: string)`, `RowBuildContext.EvaluateAsString(expression: string)`                                     |
| `DefaultExpressionEvaluator`       | `Evaluate(expression: string, context: ExpressionContext)`, `ResolveToken(tokenName: string, context: ExpressionContext)`                                                                                                                         |
| `FunctionExpressionRuntime`        | `LooksLikeFunctionExpression(expression: string)`, `Evaluate(expression: string, context: ExpressionContext)`, `EvaluateNode(node: Node, context: ExpressionContext)`, `EvaluateFunction(call: FunctionCallNode, context: ExpressionContext)` |
| `FunctionExpressionRuntime.Parser` | `ParseExpression()`, `ParseFunctionCall(name: string)`, `ParseArray()`, `ParseToken()`                                                                                                                                                        |

## Validation Path (No Execution)

```mermaid
sequenceDiagram
    participant Caller as Caller
    participant Validator as DefaultExpressionValidator IExpressionValidator
    participant Runtime as FunctionExpressionRuntime

    Caller->>Validator: 1 Validate(expression string)
    alt 2 Function like expression
        Validator->>Runtime: 3 ValidateFunctionExpression(expression string)
        Runtime-->>Validator: 4 ExpressionValidationResult
    else 2 Token or literal expression
        Validator->>Validator: 3 Validate token brace shape and token regex
        Validator-->>Caller: 4 ExpressionValidationResult
    end
    Validator-->>Caller: 5 ExpressionValidationResult
```

## Validation Walkthrough

1. DefaultExpressionValidator receives raw expression text.
2. It classifies the input as function-like or token/literal style.
3. Function-like expressions are parsed and semantically validated for known functions and argument counts.
4. Non-function expressions are checked for token-brace structure and placeholder format.
5. A single ExpressionValidationResult is returned with IsValid and error messages.

## Key Source References

- [DefaultExpressionEvaluator](../src/Core/KineticReports.Core/Engine/Expressions/DefaultExpressionEvaluator.cs)
- [FunctionExpressionRuntime](../src/Core/KineticReports.Core/Engine/Expressions/FunctionExpressionRuntime.cs)
- [DefaultExpressionValidator](../src/Core/KineticReports.Core/Engine/Expressions/DefaultExpressionValidator.cs)
- [DefaultReportBuilder](../src/Core/KineticReports.Core/Engine/Building/DefaultReportBuilder.cs)
