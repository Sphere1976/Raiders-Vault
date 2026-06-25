package com.raidersvault.liveops.bdd;

import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;
import org.springframework.http.ResponseEntity;
import org.springframework.web.client.RestTemplate;

import java.util.List;
import java.util.Map;

import static org.assertj.core.api.Assertions.assertThat;

public class LiveOpsStepDefinitions extends CucumberSpringConfiguration {
    private ResponseEntity<Map> response;

    @When("the client requests the live operations snapshot")
    public void requestLiveOperationsSnapshot() {
        var restTemplate = new RestTemplate();
        response = restTemplate.getForEntity("http://localhost:" + port + "/api/v1/live-ops", Map.class);
    }

    @Then("the response status should be {int}")
    public void responseStatusShouldBe(int statusCode) {
        assertThat(response.getStatusCode().value()).isEqualTo(statusCode);
    }

    @Then("the response should include at least {int} active condition")
    public void responseShouldIncludeActiveConditions(int minimumCount) {
        var activeConditions = (List<?>) response.getBody().get("activeConditions");
        assertThat(activeConditions).hasSizeGreaterThanOrEqualTo(minimumCount);
    }

    @Then("the response should include at least {int} news update")
    public void responseShouldIncludeNewsUpdates(int minimumCount) {
        var newsUpdates = (List<?>) response.getBody().get("newsUpdates");
        assertThat(newsUpdates).hasSizeGreaterThanOrEqualTo(minimumCount);
    }
}
