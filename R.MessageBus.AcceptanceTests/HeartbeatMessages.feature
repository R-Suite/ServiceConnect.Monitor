Feature: HeartbeatMessages
	In order to monitor a service endpoint
	As a user of the monitoring web application
	I need to be able to view service information

Scenario: Heartbeat messages are inserted into database
	Given I receive heartbeat messages
	When I query the heartbeat database collection
	Then the heartbeat messages should exist in the database

Scenario: Service is inserted into the database
	Given I receive heartbeat messages
	When I query the service database collection
	Then the service should exist in the database