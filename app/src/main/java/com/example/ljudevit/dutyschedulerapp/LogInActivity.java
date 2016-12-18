package com.example.ljudevit.dutyschedulerapp;

import android.app.ProgressDialog;
import android.content.Intent;
import android.content.SharedPreferences;
import android.database.Cursor;
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
import android.widget.TextView;
import android.widget.Toast;

public class LogInActivity extends AppCompatActivity {

    String LOGIN_PREFERENCE_INFO = "logInPreferences";
    private EditText userName;
    private EditText password;
    private SharedPreferences logInPref;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_log_in);


        userName = (EditText) findViewById(R.id.userName);
        password = (EditText) findViewById(R.id.password);
        final CheckBox remember = (CheckBox) findViewById(R.id.checkbox);

        logInPref = getSharedPreferences(LOGIN_PREFERENCE_INFO, MODE_PRIVATE);
        userName.setText(logInPref.getString("logInUser", ""));
        password.setText(logInPref.getString("logInPassword", ""));
        if(userName.getText().length()>0) {

            remember.setChecked(true);
        }

        if(remember.isChecked()) {
            logIn();
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
        final ProgressDialog progressDialog = new ProgressDialog(LogInActivity.this);
        progressDialog.setIndeterminate(true);
        progressDialog.setMessage("Logging in...");
        progressDialog.show();

        Toast.makeText(getApplicationContext(), "Logged as " + userName.getText() + " successfully!", Toast.LENGTH_SHORT).show();

        SharedPreferences.Editor editor = logInPref.edit();
        editor.clear();

        CheckBox remember = (CheckBox) findViewById(R.id.checkbox);
        if (remember.isChecked()) {
            editor.putString("logInUser", userName.getText().toString());
            editor.putString("logInPassword", password.getText().toString());
            editor.apply();
        }

        Intent intent = new Intent(getApplicationContext(), CalendarActivity.class);
        startActivity(intent);
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
