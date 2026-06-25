package com.raidersvault.liveops.bdd;

import com.raidersvault.liveops.RaidersVaultLiveOpsApplication;
import io.cucumber.spring.CucumberContextConfiguration;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.server.LocalServerPort;

@CucumberContextConfiguration
@SpringBootTest(
        classes = RaidersVaultLiveOpsApplication.class,
        webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT)
public class CucumberSpringConfiguration {
    @LocalServerPort
    protected int port;
}
