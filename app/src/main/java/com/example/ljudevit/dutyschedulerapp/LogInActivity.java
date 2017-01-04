package com.example.ljudevit.dutyschedulerapp;

import android.app.ProgressDialog;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.database.Cursor;
import android.os.AsyncTask;
import android.os.Bundle;
import android.support.design.widget.FloatingActionButton;
import android.support.design.widget.Snackbar;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.View;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.TextView;
import android.widget.Toast;

import java.util.concurrent.ExecutionException;

public class LogInActivity extends AppCompatActivity {

    String LOGIN_PREFERENCE_INFO = "appPreferences";
    private EditText userName;
    private EditText password;
    private SharedPreferences logInPref;
    DBHelper logInDB;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_log_in);

        logInPref = getSharedPreferences(LOGIN_PREFERENCE_INFO, MODE_PRIVATE);
        Boolean useDB = logInPref.getBoolean("localDB",false);
        //punjenje baze tesnim primjerima
        if(useDB) {
            logInDB = new DBHelper(this);
            //popuniBazu();
        }

        userName = (EditText) findViewById(R.id.userName);
        password = (EditText) findViewById(R.id.password);
        final CheckBox remember = (CheckBox) findViewById(R.id.checkbox);

        ImageButton settings = (ImageButton) findViewById(R.id.login_settings_button);

        settings.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent preferences = new Intent(getApplicationContext(), LogInPreferences.class);
                startActivity(preferences);
            }
        });

        userName.setText(logInPref.getString("rememberedName", ""));
        password.setText(logInPref.getString("rememberedPass", ""));
        if(userName.getText().length()>0) {
            remember.setChecked(true);
        }

        Button signIn = (Button) findViewById(R.id.signIn);
        signIn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {

                logIn();
            }
        });
    }

    private void logIn(){
        //prikaz
        final ProgressDialog logginDialog = new ProgressDialog(LogInActivity.this);
        logginDialog.setIndeterminate(true);
        logginDialog.setMessage("Logging in...");
        logginDialog.show();

        //Postavke ulogiranog korisnika
        String usersName = userName.getText().toString();
        String usersPass = password.getText().toString();

        final String address = logInPref.getString("mainURL","http://10.150.150.2:5000");
        User loggedInUser = new HttpHandler().logIn(address,usersName,usersPass);
        Toast.makeText(getApplicationContext(), loggedInUser.toString(), Toast.LENGTH_SHORT).show();
        if(!loggedInUser.getUsername().isEmpty()) {
            //spremanje login podataka
            CheckBox remember = (CheckBox) findViewById(R.id.checkbox);
            if (remember.isChecked()) {
                SharedPreferences.Editor editor = logInPref.edit();
                editor.clear();
                editor.putString("mainURL", address);
                editor.putString("rememberedPass", usersPass);
                editor.putString("rememberedUserName", usersName);
                editor.putString("rememberedName", loggedInUser.getName());
                editor.putString("rememberedSurname", loggedInUser.getSurname());
                editor.putString("rememberedPhone", loggedInUser.getPhone());
                editor.putString("rememberedOffice", loggedInUser.getOffice());
                editor.apply();
            }

            Intent intent = new Intent(getApplicationContext(), CalendarActivity.class);
            intent.putExtra("loggedInUser", loggedInUser);
            startActivity(intent);
            Toast.makeText(getApplicationContext(), "Logged as " + usersName + " successfully!", Toast.LENGTH_SHORT).show();
            logginDialog.hide();
        }
        else{
            Toast.makeText(getApplicationContext(), "Incorrect username or password!", Toast.LENGTH_SHORT).show();
        }
    }


    private void popuniBazu(){


        logInDB.insertUser(1, "user1", "Irena", "Oršolić", "iorsolic@fer.hr", "12345678", "C07-16", 255);
        logInDB.insertUser(2, "user2", "Sanja", "Grubeša", "sgrubesa@fer.hr", "12345678", "C07-16", 255);
        logInDB.insertUser(3, "user3", "Darko", "Štriga", "dstriga@fer.hr", "12345678", "C08-03", 231);
        logInDB.insertUser(4, "user4", "Tomislav", "Grgić", "tgrgic@fer.hr", "12345678", "C07-16", 255);
        logInDB.insertUser(5, "user5", "Aleksandar", "Antonić", "aantonic@fer.hr", "12345678", "C08-19", 218);
        logInDB.insertUser(6, "user6", "Dario", "Pevec", "dpevec@fer.hr", "12345678", "C07-01", 269);
        logInDB.insertUser(7, "user7", "Pavle", "Skočir", "pskocir@fer.hr", "12345678", "C08-04", 338);
        logInDB.insertUser(8, "user8", "Matija", "Šulc", "msulc@fer.hr", "12345678", "C08-21", 343);
        logInDB.insertUser(9, "user9", "Mirko", "Sužnjević", "msuznjevic@fer.hr", "12345678", "C07-16", 255);
        logInDB.insertUser(10, "user10", "Jurica" ,"Babić", "jbabic@fer.hr", "12345678", "C07-01", 269);
        logInDB.insertUser(11, "user11", "Ivan", "Silvar", "isilvar@fer.hr", "12345678", "C07-01", 269);
        logInDB.insertUser(12, "user12", "Marko", "Pavečić", "mpavelic@fer.hr", "12345678", "C07-18", 592);
        logInDB.insertUser(13, "user13", "Damjan", "Katušić", "dkatusic@fer.hr", "12345678", "C08-04", 338);
        logInDB.insertUser(14, "user14", "Ivana", "Rašan", "irasan@fer.hr", "12345678", "C07-01", 269);
        logInDB.insertUser(15, "user15", "Martina", "Manhart", "mmanhart@fer.hr", "12345678", "C06-01", 377);
        logInDB.insertUser(16, "user16", "Nenad", "Markuš", "nmarkus@fer.hr", "12345678", "C06-01", 377);
        logInDB.insertUser(17, "user17", "Petar", "Krivić", "pkrivic@fer.hr", "12345678", "C08-19", 218);
        logInDB.insertUser(18, "user18", "Renato", "Šoić", "rsoic@fer.hr", "12345678", "C08-04", 338);
        logInDB.insertUser(19, "user19", "Ivan", "Gogić", "igogic@fer.hr", "12345678", "C06-01", 377);
        logInDB.insertUser(20, "user20", "Marina", "Bagić Babac", "mbbabac@fer.hr", "12345678", "C08-06", 268);
        logInDB.insertUser(21, "user21", "Matija", "Džanko", "mdzanko@fer.hr", "12345678", "C07-04", 158);
        logInDB.insertUser(22, "user22", "Denis", "Salopek", "dsalopek@fer.hr", "12345678", "C07-18", 592);
        logInDB.insertUser(23, "user23", "Martina", "Marjanović", "mmarjanovic@fer.hr", "12345678", "C08-19", 218);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_log_in, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }
}
