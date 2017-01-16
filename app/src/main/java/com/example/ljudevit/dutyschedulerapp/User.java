package com.example.ljudevit.dutyschedulerapp;

import java.io.Serializable;

/**
 * class implementation of single user
 * class contains users name, surname, phone number and room
 */

class User implements Serializable{
    private Integer ID;
    private String username;
    private String email;
    private String password;
    private String name;
    private String surname;
    private String phone;
    private String office;
    private Boolean isAdmin;
    private String cookie;

    public String getCookie() {
        return cookie;
    }

    public void setCookie(String cookie) {
        this.cookie = cookie;
    }

    public Boolean getAdmin() {
        return isAdmin;
    }

    public void setAdmin(Boolean admin) {
        isAdmin = admin;
    }

    public Integer getID() {
        return ID;
    }

    void setID(Integer ID) {
        this.ID = ID;
    }

    String getUsername() {
        return username;
    }

    void setUsername(String username) {
        this.username = username;
    }

    public String getEmail() {
        return email;
    }

    void setEmail(String email) {
        this.email = email;
    }

    public String getPassword() {
        return password;
    }

    public void setPassword(String password) {
        this.password = password;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    String getSurname() {
        return surname;
    }

    void setSurname(String surname) {
        this.surname = surname;
    }

    String getPhone() {
        return phone;
    }

    void setPhone(String phone) {
        this.phone = phone;
    }

    String getOffice() {
        return office;
    }

    void setOffice(String office) {
        this.office = office;
    }
}
