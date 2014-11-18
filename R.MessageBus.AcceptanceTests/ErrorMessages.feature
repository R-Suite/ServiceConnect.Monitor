Feature: ErrorMessages
	In order to analyse failed messages
	As a user of the monitoring web application
	I need to be able to view errored messages

Scenario: Error Messages are inserted into database
	Given I receive error messages
	When I query the error database collection
	Then the error messages should exist in the database