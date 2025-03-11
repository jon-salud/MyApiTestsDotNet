Feature: API Authentication and Data Retrieval
  As a developer
  I want to authenticate with an API key for the comics endpoint
  So that I can retrieve comics protected data

  Scenario: Successfully retrieve comics data with valid API key
    Given I have valid Marvel API credentials
    When I send a GET request to "comics" endpoint
    Then I receive a 200 status code
    And the response contains "comics" data
    And the first comic book has a title of "Marvel Previews (2017)"

  Scenario: Successfully retrieve characters data with valid API key
    Given I have valid Marvel API credentials
    When I send a GET request to "characters" endpoint
    Then I receive a 200 status code
    And the response contains "characters" data
    And the first character has a name of "3-D Man"
    
  Scenario: Fail to retrieve comics with invalid API key
    Given I have invalid Marvel API credentials
    When I send a GET request to "comics" endpoint
    Then I receive a 401 status code
  
