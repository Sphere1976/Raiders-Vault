package com.raidersvault.liveops.controller;

import com.raidersvault.liveops.model.LiveOpsResponse;
import com.raidersvault.liveops.service.LiveOpsService;
import org.springframework.http.CacheControl;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.time.Duration;

@RestController
@RequestMapping("/api/v1/live-ops")
public class LiveOpsController {
    private final LiveOpsService liveOpsService;

    public LiveOpsController(LiveOpsService liveOpsService) {
        this.liveOpsService = liveOpsService;
    }

    @GetMapping
    public ResponseEntity<LiveOpsResponse> getLiveOps() {
        return ResponseEntity.ok()
                .cacheControl(CacheControl.maxAge(Duration.ofSeconds(30)).cachePublic())
                .body(liveOpsService.currentSnapshot());
    }
}
