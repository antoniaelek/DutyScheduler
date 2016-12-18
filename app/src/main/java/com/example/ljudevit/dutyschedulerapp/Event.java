package com.example.ljudevit.dutyschedulerapp;

import java.util.Date;

/**
 * class implementation of a single duty service
 * class contains duty's date, executor, remark and sentry's replacement
 */

public class Event {
    private Date eventTime;
    private User sentry;
    private String remark;
    private User replacement;

    public Date getEventTime() {
        return eventTime;
    }

    public void setEventTime(Date eventTime) {
        this.eventTime = eventTime;
    }

    public User getSentry() {
        return sentry;
    }

    public void setSentry(User sentry) {
        this.sentry = sentry;
    }

    public String getRemark() {
        return remark;
    }

    public void setRemark(String remark) {
        this.remark = remark;
    }

    public User getReplacement() {
        return replacement;
    }

    public void setReplacement(User replacement) {
        this.replacement = replacement;
    }
}
