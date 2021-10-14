Feature: StringProcessor

Simplest string processor

@tag1
Scenario: Uppercase
	Given Input string is "abc"
	When Uppercase it
	Then the string result should be "ABC"
