Feature: Longer

A example for parallel long operation with certain timeout (must be in separate feature)

@wait_5000
Scenario: Wait 5000
	When do long work for 5000
	Then just void