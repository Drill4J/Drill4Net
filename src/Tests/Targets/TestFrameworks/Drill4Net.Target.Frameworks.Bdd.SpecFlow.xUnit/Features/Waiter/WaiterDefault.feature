Feature: WaiterDefault

A example for parallel long operation with default timeout (must be in separate feature)

@default_wait
Scenario: Wait default
	When do default long work
	Then just void
