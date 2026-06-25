package com.raidersvault.liveops.service;

import com.raidersvault.liveops.model.LiveOpsResponse;
import com.raidersvault.liveops.model.MapCondition;
import com.raidersvault.liveops.model.NewsUpdate;
import org.springframework.stereotype.Service;

import java.time.Instant;
import java.time.LocalDate;
import java.util.List;

@Service
public class LiveOpsService {
    public LiveOpsResponse currentSnapshot() {
        return new LiveOpsResponse(
                Instant.now(),
                "Raiders Vault Spring Boot LiveOps Service",
                List.of(
                        new MapCondition(
                                "The Blue Gate",
                                "Harvester",
                                "Active",
                                "Current rotation",
                                "Prioritize extraction routes, rare mechanical drops, and squad communication."),
                        new MapCondition(
                                "Dam Battlegrounds",
                                "Night Raid",
                                "Watch",
                                "Upcoming rotation",
                                "Use conservative kits and bring escape utility for low-visibility routes.")),
                List.of(
                        new NewsUpdate(
                                "LiveOps service online",
                                LocalDate.now(),
                                "Raiders Vault",
                                "https://github.com/",
                                "Spring Boot service exposes a typed REST contract for downstream dashboards.")));
    }
}
