package com.raidersvault.liveops.model;

public record MapCondition(
        String map,
        String condition,
        String status,
        String timeWindow,
        String recommendation) {
}
