package com.raidersvault.gateway.model;

import java.util.List;

public record GlobalOpsSummary(
        String generatedAt,
        int activeConditionCount,
        int newsUpdateCount,
        List<String> executiveSignals) {
}
