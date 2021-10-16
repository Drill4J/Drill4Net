Feature: Longer

A premier for parallel long operation with certain timeout (must be in separate feature)

@timeout_5000
Scenario: Wait 5000
	Given the timeout is 5000
	When do long work
	Then the int result should be 0
