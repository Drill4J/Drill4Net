Feature: StringProcessor

Simplest string processor

@uppercase
Scenario: Uppercase
	Given Input string is "abc"
	When Uppercase it
	Then the string result should be "ABC"
