package com.example.ljudevit.dutyschedulerapp;

import android.content.SharedPreferences;
import android.support.annotation.NonNull;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.Button;
import android.widget.CalendarView;
import android.widget.ImageButton;
import android.widget.PopupWindow;
import android.view.ViewGroup.LayoutParams;
import android.widget.TextView;
import android.widget.Toast;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;
import java.util.concurrent.ExecutionException;

public class CalendarActivity extends AppCompatActivity {

    String CALENDAR_PREFERENCE_INFO = "appPreferences";
    private SharedPreferences calPref;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_calendar);
        calPref = getSharedPreferences(CALENDAR_PREFERENCE_INFO, MODE_PRIVATE);
        String hostURL = calPref.getString("mainURL","http://10.150.150.2:5000");
        final View parent = this.findViewById(R.id.calendar);
        final CalendarView calendar = (CalendarView) findViewById(R.id.calendar);

        Calendar cal = Calendar.getInstance();
        List<Schedule> currentMonth = new ArrayList<>();
        try {
            currentMonth = new HttpHandler().monthDates(hostURL+"/api/Calendar",cal.get(Calendar.MONTH)+1,cal.get(Calendar.YEAR));
        } catch (ExecutionException | InterruptedException e) {
            e.printStackTrace();
        }
        for(Schedule date : currentMonth){
            //TODO pojedini datum instancirat
        }
        User loggedInUser = (User) getIntent().getSerializableExtra("loggedInUser");
        calendar.setOnDateChangeListener(new CalendarView.OnDateChangeListener() {
            @Override
            public void onSelectedDayChange(@NonNull CalendarView calendarView, int year, int month, int dayOfMonth) {

                LayoutInflater inflater = (LayoutInflater) getApplicationContext().getSystemService(LAYOUT_INFLATER_SERVICE);

                // Inflate the custom layout/view
                View eventView = inflater.inflate(R.layout.single_event,calendar);


                TextView selectedDate = (TextView) eventView.findViewById(R.id.date);
                selectedDate.setText(String.valueOf(dayOfMonth)+"."+String.valueOf(month+1)+"."+String.valueOf(year)+".");
                final PopupWindow singleDateInfo = new PopupWindow(eventView,
                        LayoutParams.WRAP_CONTENT,
                        LayoutParams.WRAP_CONTENT);

                singleDateInfo.showAtLocation(parent,Gravity.CENTER,0,0);
                ImageButton close = (ImageButton) eventView.findViewById(R.id.closeButton);
                close.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View view) {
                        singleDateInfo.dismiss();
                    }
                });

                //ako je ulogirani isti koji je dezuran taj dan i ako nema već zamjenu
                final Button traziZamjenu = (Button) eventView.findViewById(R.id.traziZamjenuButton);

            }
        });
    }
}
