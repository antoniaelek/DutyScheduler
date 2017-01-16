package com.example.ljudevit.dutyschedulerapp;

import java.util.Date;

/**
 * Created by Ljudevit on 15.1.2017..
 */

public class Offer {
    private Integer id;
    private String userName;
    private Date date;
    private Boolean isReplaceble;

    public Integer getId() {
        return id;
    }

    public void setId(Integer id) {
        this.id = id;
    }

    public String getUserName() {
        return userName;
    }

    public void setUserName(String userName) {
        this.userName = userName;
    }

    public Date getDate() {
        return date;
    }

    public void setDate(Date date) {
        this.date = date;
    }

    public Boolean getReplaceble() {
        return isReplaceble;
    }

    public void setReplaceble(Boolean replaceble) {
        isReplaceble = replaceble;
    }
}
