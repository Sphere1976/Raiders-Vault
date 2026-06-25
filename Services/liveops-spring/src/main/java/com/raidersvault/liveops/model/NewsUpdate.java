package com.raidersvault.liveops.model;

import java.time.LocalDate;

public record NewsUpdate(
        String title,
        LocalDate publishedAt,
        String source,
        String url,
        String summary) {
}
