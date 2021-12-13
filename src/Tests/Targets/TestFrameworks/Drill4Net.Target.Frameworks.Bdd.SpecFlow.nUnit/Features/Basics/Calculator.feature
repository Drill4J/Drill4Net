Feature: Calculator
![Calculator](https://specflow.org/wp-content/uploads/2020/09/calculator.png)
Simple calculator for adding and substracting **two** numbers

Link to a feature: [Calculator](Drill4Net.Target.Frameworks.Bdd.SpecFlow.xUnit/Features/Calculator.feature)
***Further read***: **[Learn more about how to generate Living Documentation](https://docs.specflow.org/projects/specflow-livingdoc/en/latest/LivingDocGenerator/Generating-Documentation.html)**

@add
Scenario: Add two numbers
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	Then the int result should be 120

@substract
Scenario: Substract two numbers
	Given the first number is 50
	And the second number is 24
	When the two numbers are substracted
	Then the int result should be 26

@wrongResult
Scenario: Substracting is wrong
	Given the first number is 4
	And the second number is 2
	When the two numbers are substracted
	Then the int result should be 0

@wrongDefinition
Scenario: Wrong definition
	Given the first number is 4
	And the second number is 2
	When the two numbers are substracted
	Then non-existent definition