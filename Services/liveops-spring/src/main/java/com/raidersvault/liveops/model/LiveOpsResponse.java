package com.raidersvault.liveops.model;

import java.time.Instant;
import java.util.List;

public record LiveOpsResponse(
        Instant generatedAt,
        String source,
        List<MapCondition> activeConditions,
        List<NewsUpdate> newsUpdates) {
}
