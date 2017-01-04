package com.example.ljudevit.dutyschedulerapp;

import java.io.Serializable;
import java.util.Date;

class Schedule implements Serializable {
    private String type;
    private String name;
    private String date;
    private String scheduled;
    private Boolean isReplaceable;

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

    public String getDate() {
        return date;
    }

    void setDate(String date) {
        this.date = date;
    }

    public String getScheduled() {
        return scheduled;
    }

    void setScheduled(String scheduled) {
        this.scheduled = scheduled;
    }

    public Boolean getReplaceable() {
        return isReplaceable;
    }

    void setReplaceable(Boolean replaceable) {
        isReplaceable = replaceable;
    }
}