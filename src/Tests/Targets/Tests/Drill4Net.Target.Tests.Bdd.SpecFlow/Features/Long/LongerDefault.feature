Feature: LongerDefault

A premier for parallel long operation with default timeout (must be in separate feature)

@default_5000
Scenario: Wait 5000 default
	When do default long work
	Then just void
