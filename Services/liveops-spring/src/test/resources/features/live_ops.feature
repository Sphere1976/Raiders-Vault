Feature: Live operations API
  Product and operations users need a reliable live-ops endpoint so dashboards can
  show current map conditions and operational news.

  Scenario: Retrieve active live operations snapshot
    When the client requests the live operations snapshot
    Then the response status should be 200
    And the response should include at least 1 active condition
    And the response should include at least 1 news update
