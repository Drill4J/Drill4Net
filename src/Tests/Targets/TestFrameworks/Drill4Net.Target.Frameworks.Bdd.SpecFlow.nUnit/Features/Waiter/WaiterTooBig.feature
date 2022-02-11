Feature: WaiterTooBig

A example for parallel long operation with certain timeout (must be in separate feature)

@wait_if_100000
Scenario: Wait if too big
	When do long work for 100000
	Then just void
