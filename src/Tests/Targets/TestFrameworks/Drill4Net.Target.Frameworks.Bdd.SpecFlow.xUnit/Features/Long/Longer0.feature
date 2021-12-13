Feature: Longer0

A example for parallel long operation with certain timeout (must be in separate feature)

@wait_if_0
Scenario: Wait if zero value
	When do long work for 0
	Then just void
