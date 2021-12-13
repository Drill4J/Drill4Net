Feature: StringProcessor

Simplest string processor

@uppercase
Scenario: Uppercase
	Given Input string is "abc"
	When Uppercase it
	Then the string result should be "ABC"

@uppercase_empty
Scenario: UppercaseEmpty
	Given Input string is ""
	When Uppercase it
	Then the string result should be ""

@uppercase_null
Scenario: UppercaseNull
	Given Input string is NULL
	When Uppercase it
	Then the string result should be NULL
