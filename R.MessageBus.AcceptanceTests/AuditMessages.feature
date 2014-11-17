Feature: AuditMessages
	In order to analyse messagebus activity
	As a user of the monitoring web application
	I need to be able to view audit queue messages

Scenario: Audit Messages are inserted into database
	Given I receive audit messages
	When I query the audit database collection
	Then the audit messages should exist in the database

Scenario: Destination and Source Service Exist in database
	Given I receive audit messages
	When I query the service database collection
	Then the destination service should exist in the database
	And the source service should exist in the database

Scenario: Message type should exist in the database
	Given I receive audit messages
	When I query the service message database collection
	Then the message type connecting destination and source should exist in the database