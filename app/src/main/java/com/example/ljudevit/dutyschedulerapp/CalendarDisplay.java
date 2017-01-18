package com.example.ljudevit.dutyschedulerapp;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.design.widget.NavigationView;
import android.support.v4.view.GravityCompat;
import android.support.v4.widget.DrawerLayout;
import android.support.v7.app.ActionBarDrawerToggle;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;

public class CalendarDisplay extends AppCompatActivity
        implements NavigationView.OnNavigationItemSelectedListener {

    String CALENDAR_PREFERENCE_INFO = "appPreferences";
    private SharedPreferences calPref;
    User loggedInUser;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_calendar_display);
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        loggedInUser = (User) getIntent().getSerializableExtra("loggedInUser");

        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout);
        ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(
                this, drawer, toolbar, R.string.navigation_drawer_open, R.string.navigation_drawer_close);
        drawer.setDrawerListener(toggle);
        toggle.syncState();

        NavigationView navigationView = (NavigationView) findViewById(R.id.nav_view);
        navigationView.setNavigationItemSelectedListener(this);

        calPref = getSharedPreferences(CALENDAR_PREFERENCE_INFO, MODE_PRIVATE);
        String hostURL = calPref.getString("mainURL","");
        String logedUser = calPref.getString("loginUser","");
        String cookie = calPref.getString("cookie","");

        CalendarView calendar = (CalendarView) findViewById(R.id.calendar_view);
        final View parent = this.findViewById(R.id.calendar_view);
        calendar.assignValues(hostURL,cookie,logedUser,loggedInUser.getAdmin());
    }

    @Override
    public void onBackPressed() {
        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout);
        if (drawer.isDrawerOpen(GravityCompat.START)) {
            drawer.closeDrawer(GravityCompat.START);
        } else {
            super.onBackPressed();
        }
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.

        getMenuInflater().inflate(R.menu.calendar_display, menu);
        return true;
    }


    @SuppressWarnings("StatementWithEmptyBody")
    @Override
    public boolean onNavigationItemSelected(@NonNull MenuItem item) {
        // Handle navigation view item clicks here.
        int id = item.getItemId();

        if (id == R.id.nav_statistics) {
            Intent intent = new Intent(getApplicationContext(), Statistics.class);
            startActivity(intent);
        } else if (id == R.id.nav_account) {
            Intent intent = new Intent(getApplicationContext(), AccountActivity.class);
            intent.putExtra("loggedInUser", loggedInUser);
            startActivity(intent);

        } else if (id == R.id.nav_logout) {
            Intent intent = new Intent(getApplicationContext(), LogInActivity.class);
            startActivity(intent);
        }

        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout);
        drawer.closeDrawer(GravityCompat.START);
        return true;
    }
}
