package com.example.ljudevit.dutyschedulerapp;

import android.content.SharedPreferences;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

public class AccountActivity extends AppCompatActivity {

    String CALENDAR_PREFERENCE_INFO = "appPreferences";
    private SharedPreferences calPref;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_account);

        calPref = getSharedPreferences(CALENDAR_PREFERENCE_INFO, MODE_PRIVATE);
        final String url = calPref.getString("mainURL","");
        final String cookie = calPref.getString("cookie","");

        final User accountInfo = (User) getIntent().getSerializableExtra("loggedInUser");
        final EditText name = (EditText) findViewById(R.id.name);
        final EditText surname = (EditText) findViewById(R.id.lastName);
        final EditText office = (EditText) findViewById(R.id.office);
        final EditText phone = (EditText) findViewById(R.id.phone);
        final EditText email = (EditText) findViewById(R.id.email);
        final EditText password = (EditText) findViewById(R.id.password);
        Button update = (Button) findViewById(R.id.update_button);

        name.setText(accountInfo.getName());
        surname.setText(accountInfo.getSurname());
        office.setText(accountInfo.getOffice());
        phone.setText(accountInfo.getPhone());
        email.setText(accountInfo.getEmail());
        password.setText(accountInfo.getPassword());

        update.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                accountInfo.setName(name.getText().toString());
                accountInfo.setSurname(surname.getText().toString());
                accountInfo.setOffice(office.getText().toString());
                accountInfo.setPhone(phone.getText().toString());
                accountInfo.setEmail(email.getText().toString());
                accountInfo.setPassword(password.getText().toString());

                new HttpHandler().updateInfo(url,cookie,accountInfo);
                Toast.makeText(getApplicationContext(),"Podatci uspješno ažurirani",Toast.LENGTH_LONG);
            }
        });


    }
}
