package com.example.ljudevit.dutyschedulerapp;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;

class DBHelper extends SQLiteOpenHelper {

    //region VARIABLES
    private static final String DATABASE_NAME = "MyDBName.db";

    private static final String TABLE_USER = "User";
    private static final String COLUMN_USERNAME = "username";
    private static final String COLUMN_EMAIL = "email";
    private static final String COLUMN_PASSWORD = "password";
    private static final String COLUMN_NAME = "name" ;
    private static final String COLUMN_SURNAME = "surname";
    private static final String COLUMN_OFFICE = "office";
    private static final String COLUMN_PHONE = "phone";

    private static final String TABLE_SCHEDULE = "Schedule";
    private static final String COLUMN_DATE = "date";
    private static final String COLUMN_ISSPECIAL = "isSpecial";
    private static final String COLUMN_NOTE= "note";

    private static final String TABLE_REPLACEMENT = "Replacement";
    private static final String COLUMN_REPLACEMENT_ID = "replacementID";

    /*shared columns*/

    private static final String COLUMN_USER_ID = "userID";
    private static final String COLUMN_SCHEDULE_ID = "scheduleID";
    //endregion

    DBHelper(Context context)
    {
        super(context, DATABASE_NAME , null, 1);
    }

    @Override
    public void onCreate(SQLiteDatabase db) {
        db.execSQL(
                "create table " + TABLE_USER +
                        "(" + COLUMN_USER_ID + " integer primary key not null,"
                        + COLUMN_USERNAME + " text not null,"
                        + COLUMN_EMAIL + " text not null,"
                        + COLUMN_PASSWORD + " text not null,"
                        + COLUMN_NAME + " text not null,"
                        + COLUMN_SURNAME + " text not null,"
                        + COLUMN_OFFICE + " text not null,"
                        + COLUMN_PHONE + " text not null);"
        );
        db.execSQL(
                "create table " + TABLE_SCHEDULE +
                        "(" + COLUMN_SCHEDULE_ID + " integer primary key not null,"
                        + COLUMN_DATE + " text not null,"
                        + COLUMN_ISSPECIAL + " integer not null,"
                        + COLUMN_NOTE + " text,"
                        + COLUMN_USER_ID + " integer not null," +
                        " foreign key ("+ COLUMN_USER_ID +") references " + TABLE_USER + "(" + COLUMN_USER_ID + "));"
        );

        db.execSQL(
                "create table " + TABLE_REPLACEMENT +
                        "(" + COLUMN_REPLACEMENT_ID + " integer primary key not null,"
                        + COLUMN_SCHEDULE_ID + " integer not null,"
                        + COLUMN_USER_ID + " integer not null," +
                        " foreign key ("+ COLUMN_SCHEDULE_ID +") references " + TABLE_SCHEDULE + "(" + COLUMN_SCHEDULE_ID + "),"+
                        " foreign key ("+ COLUMN_USER_ID +") references " + TABLE_USER + "(" + COLUMN_USER_ID + "));"
        );

    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        db.execSQL("DROP TABLE IF EXISTS " + TABLE_USER);
        db.execSQL("DROP TABLE IF EXISTS " + TABLE_SCHEDULE);
        db.execSQL("DROP TABLE IF EXISTS " + TABLE_REPLACEMENT);
        onCreate(db);
    }

    //region CRUDE User table
    boolean insertUser(int userID, String username, String name, String surname, String email, String password, String office, Integer phone)
    {
        SQLiteDatabase db = this.getWritableDatabase();
        ContentValues contentValues = new ContentValues();
        contentValues.put(COLUMN_USER_ID, userID);
        contentValues.put(COLUMN_USERNAME, username);
        contentValues.put(COLUMN_NAME, name);
        contentValues.put(COLUMN_SURNAME, surname);
        contentValues.put(COLUMN_EMAIL, email);
        contentValues.put(COLUMN_PASSWORD, password);
        contentValues.put(COLUMN_OFFICE, office);
        contentValues.put(COLUMN_PHONE, phone);
        db.insert(TABLE_USER, null, contentValues);
        return true;
    }

    public Integer deleteUser (Integer userID)
    {
        SQLiteDatabase db = this.getWritableDatabase();
        return db.delete(TABLE_USER,
                COLUMN_USER_ID + " = ? ",
                new String[] { Integer.toString(userID) });
    }

    public Integer checkPassword(String userName, String password){
        SQLiteDatabase db = this.getReadableDatabase();
        Cursor res =  db.rawQuery( "select * from " + TABLE_USER + " where " + COLUMN_USERNAME + "='" + userName + "' and "+COLUMN_PASSWORD +" = '"+password+"'", null );
        if(res.getCount()<1) {
            res.close();
            return -1;
        }
        res.moveToFirst();
        Integer userID =res.getInt(res.getColumnIndex(DBHelper.COLUMN_USER_ID));
        res.close();
        return userID;
    }

    public User getUser(int userID){
        User selected = new User();
        SQLiteDatabase db = this.getReadableDatabase();
        Cursor res =  db.rawQuery( "select * from " + TABLE_USER + " where " + COLUMN_USER_ID + "=" + userID + "", null );
        if(res.getCount()<1) {
            res.close();
            selected.setID(0);
            return selected;
        }
        res.moveToFirst();
        selected.setID(res.getInt(res.getColumnIndex(DBHelper.COLUMN_USER_ID)));
        selected.setUsername(res.getString(res.getColumnIndex(DBHelper.COLUMN_USERNAME)));
        selected.setEmail(res.getString(res.getColumnIndex(DBHelper.COLUMN_EMAIL)));
        selected.setName(res.getString(res.getColumnIndex(DBHelper.COLUMN_NAME)));
        selected.setSurname(res.getString(res.getColumnIndex(DBHelper.COLUMN_SURNAME)));
        selected.setOffice(res.getString(res.getColumnIndex(DBHelper.COLUMN_OFFICE)));
        selected.setPhone(res.getString(res.getColumnIndex(DBHelper.COLUMN_PHONE)));
        res.close();
        return selected;
    }
    //endregion

    //region CRUDE Schedule table
    public boolean insertScheule  (Integer scheduleID, String date, Boolean isSpecial, String note, Integer userID)
    {
        SQLiteDatabase db = this.getWritableDatabase();
        ContentValues contentValues = new ContentValues();
        contentValues.put(COLUMN_SCHEDULE_ID, scheduleID);
        contentValues.put(COLUMN_DATE, date);
        if(isSpecial) contentValues.put(COLUMN_ISSPECIAL, 1);
        else contentValues.put(COLUMN_ISSPECIAL, 0);
        contentValues.put(COLUMN_NOTE, note);
        contentValues.put(COLUMN_USER_ID, userID);
        db.insert(TABLE_SCHEDULE, null, contentValues);
        return true;
    }

    public boolean updateSchedule (Integer scheduleID, Integer userID)
    {
        SQLiteDatabase db = this.getWritableDatabase();
        ContentValues contentValues = new ContentValues();
        contentValues.put(COLUMN_USER_ID, userID);
        db.update(TABLE_SCHEDULE, contentValues, COLUMN_SCHEDULE_ID + " = ? ", new String[] { Integer.toString(scheduleID) } );
        return true;
    }

    public Integer deleteSchedule (Integer scheduleID)
    {
        SQLiteDatabase db = this.getWritableDatabase();
        return db.delete(TABLE_SCHEDULE,
                COLUMN_SCHEDULE_ID + " = ? ",
                new String[] { Integer.toString(scheduleID) });
    }
/*
    public Schedule getSchedule(Date date) throws ParseException {
        Schedule selected = new Schedule();
        SQLiteDatabase db = this.getReadableDatabase();
        Cursor res =  db.rawQuery( "select * from " + TABLE_SCHEDULE + " where " + COLUMN_DATE + "=" + date.toString() + "", null );
        if(res.getCount()<1) {
            selected = null;
            return selected;
        }
        res.moveToFirst();
        DateFormat dateFormat = new SimpleDateFormat();
        selected.setDate(dateFormat.parse(res.getString(res.getColumnIndex(DBHelper.COLUMN_DATE))));
        if(res.getString(res.getColumnIndex(DBHelper.COLUMN_EMAIL)).equals("1")) {
            selected.setSpecial(true);
        }
        else selected.setSpecial(false);
        selected.setNote(res.getString(res.getColumnIndex(DBHelper.COLUMN_NOTE)));

        Cursor sen =  db.rawQuery( "select * from " + TABLE_USER + " where " + COLUMN_USER_ID + "=" + Integer.toString(res.getInt(res.getColumnIndex(DBHelper.COLUMN_USER_ID))) + "", null );
        if(res.getCount()<1) {
            selected.setScheduleID(0);
            return selected;
        }
        sen.moveToFirst();
        User sentry = new User();
        sentry.setPhone(sen.getString(res.getColumnIndex(DBHelper.COLUMN_PHONE)));
        sentry.setName(sen.getString(res.getColumnIndex(DBHelper.COLUMN_NAME)));
        sentry.setID(sen.getInt(res.getColumnIndex(DBHelper.COLUMN_USER_ID)));
        sentry.setOffice(sen.getString(res.getColumnIndex(DBHelper.COLUMN_OFFICE)));
        sentry.setSurname(sen.getString(res.getColumnIndex(DBHelper.COLUMN_SURNAME)));
        selected.setSentry(sentry);
        return selected;
    }*/
    //endregion
}
