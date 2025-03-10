Feature: API Authentication and Data Retrieval
  As a developer
  I want to authenticate with an API key
  So that I can retrieve protected data

  Scenario: Successfully retrieve data with valid API key
    Given I have valid Marvel API credentials
    When I send a GET request to "comics" endpoint
    Then I receive a 200 status code
    And the response contains comic book data
    And the first comic book has a title of "Marvel Previews (2017)"

  Scenario: Fail to retrieve comics with invalid API key
    Given I have invalid Marvel API credentials
    When I send a GET request to "comics" endpoint
    Then I receive a 401 status code