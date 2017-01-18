package com.example.ljudevit.dutyschedulerapp;

import java.io.Serializable;
import java.util.Date;
import java.util.List;

class Schedule implements Serializable {
    private Date date;
    private String weekday;
    private String type;
    private String name;
    private Boolean isPrefered;
    private Boolean isReplaceable;
    private String shiftId;
    private List<Replacement> replacementRequests;
    private User scheduled;

    public List<Replacement> getReplacementRequests() {
        return replacementRequests;
    }

    public void setReplacementRequests(List<Replacement> replacementRequests) {
        this.replacementRequests = replacementRequests;
    }

    public String getShiftId() {
        return shiftId;
    }

    public void setShiftId(String id) {
        this.shiftId = id;
    }

    public Boolean getPrefered() {
        return isPrefered;
    }

    public void setPrefered(Boolean prefered) {
        isPrefered = prefered;
    }

    public String getWeekday() {
        return weekday;
    }

    public void setWeekday(String weekday) {
        this.weekday = weekday;
    }

    public String getType() {
        return type;
    }

    void setType(String isSpecial) {
        this.type = isSpecial;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public Date getDate() {
        return date;
    }

    void setDate(Date date) {
        this.date = date;
    }

    public User getScheduled() {
        return scheduled;
    }

    void setScheduled(User scheduled) {
        this.scheduled = scheduled;
    }

    public Boolean getReplaceable() {
        return isReplaceable;
    }

    void setReplaceable(Boolean replaceable) {
        isReplaceable = replaceable;
    }
}