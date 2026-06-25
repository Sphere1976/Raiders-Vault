package com.raidersvault.liveops;

import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;

import static org.hamcrest.Matchers.greaterThanOrEqualTo;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest
@AutoConfigureMockMvc
class LiveOpsControllerTest {
    @Autowired
    private MockMvc mockMvc;

    @Test
    void returnsTypedLiveOpsSnapshot() throws Exception {
        mockMvc.perform(get("/api/v1/live-ops"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.source").value("Raiders Vault Spring Boot LiveOps Service"))
                .andExpect(jsonPath("$.activeConditions.length()", greaterThanOrEqualTo(1)))
                .andExpect(jsonPath("$.newsUpdates.length()", greaterThanOrEqualTo(1)));
    }
}
