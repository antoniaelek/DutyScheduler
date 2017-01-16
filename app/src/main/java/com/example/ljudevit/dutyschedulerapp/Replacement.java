package com.example.ljudevit.dutyschedulerapp;

import java.util.Date;

/**
 * Created by Ljudevit on 15.1.2017..
 */

public class Replacement {
    private Integer replacementId;
    private Integer shiftId;
    private String userId;
    private Date date;
    private User user;

    public Integer getReplacementId() {
        return replacementId;
    }

    public void setReplacementId(Integer replacementId) {
        this.replacementId = replacementId;
    }

    public Integer getShiftId() {
        return shiftId;
    }

    public void setShiftId(Integer shiftId) {
        this.shiftId = shiftId;
    }

    public String getUserId() {
        return userId;
    }

    public void setUserId(String userId) {
        this.userId = userId;
    }

    public Date getDate() {
        return date;
    }

    public void setDate(Date date) {
        this.date = date;
    }

    public User getUser() {
        return user;
    }

    public void setUser(User user) {
        this.user = user;
    }
}
