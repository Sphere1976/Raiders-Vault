package com.raidersvault.gateway.controller;

import com.raidersvault.gateway.model.GlobalOpsSummary;
import org.springframework.graphql.data.method.annotation.QueryMapping;
import org.springframework.stereotype.Controller;

import java.time.Instant;
import java.util.List;

@Controller
public class GlobalOpsGraphqlController {
    @QueryMapping
    public GlobalOpsSummary globalOpsSummary() {
        return new GlobalOpsSummary(
                Instant.now().toString(),
                3,
                4,
                List.of(
                        "LiveOps feed available",
                        "Inventory readiness API protected",
                        "Next.js console online",
                        "Spring LiveOps service isolated behind service boundary"));
    }
}
