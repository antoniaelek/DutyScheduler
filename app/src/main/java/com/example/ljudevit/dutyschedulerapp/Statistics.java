package com.example.ljudevit.dutyschedulerapp;

import android.content.SharedPreferences;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import java.util.List;
import java.util.concurrent.ExecutionException;

public class Statistics extends AppCompatActivity {

    String CALENDAR_PREFERENCE_INFO = "appPreferences";
    private SharedPreferences calPref;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_statistics);

        calPref = getSharedPreferences(CALENDAR_PREFERENCE_INFO, MODE_PRIVATE);
        final String url = calPref.getString("mainURL","");
        final String cookie = calPref.getString("cookie","");

        final TextView oMe = (TextView) findViewById(R.id.ordinaryMe);
        final TextView sMe = (TextView) findViewById(R.id.specialMe);
        final TextView tMe = (TextView) findViewById(R.id.totalMe);
        final TextView oBy = (TextView) findViewById(R.id.ordinaryByMe);
        final TextView sBy = (TextView) findViewById(R.id.specialByMe);
        final TextView tBy = (TextView) findViewById(R.id.totalByMe);
        final TextView oFor = (TextView) findViewById(R.id.ordinaryForMe);
        final TextView sFor = (TextView) findViewById(R.id.specialForMe);
        final TextView tFor = (TextView) findViewById(R.id.totalForMe);

        try {
            List<String> statistics = new HttpHandler().statistics(url,cookie);
            oMe.setText(statistics.get(0));
            sMe.setText(statistics.get(1));
            tMe.setText(statistics.get(2));
            oBy.setText(statistics.get(3));
            sBy.setText(statistics.get(4));
            tBy.setText(statistics.get(5));
            oFor.setText(statistics.get(6));
            sFor.setText(statistics.get(7));
            tFor.setText(statistics.get(8));
        } catch (ExecutionException e) {
            e.printStackTrace();
        } catch (InterruptedException e) {
            e.printStackTrace();
        }


    }
}
