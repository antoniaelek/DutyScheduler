package com.example.ljudevit.dutyschedulerapp;

import android.app.Dialog;
import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Color;
import android.graphics.Typeface;
import android.support.annotation.NonNull;
import android.util.AttributeSet;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.GridView;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.PopupWindow;
import android.widget.TextView;
import android.widget.Toast;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashSet;
import java.util.List;
import java.util.concurrent.ExecutionException;

import static android.content.Context.LAYOUT_INFLATER_SERVICE;

public class CalendarView extends LinearLayout {
    // for logging
    private static final String LOGTAG = "Calendar View";

    // how many days to show, defaults to six weeks, 42 days
    private static final int DAYS_COUNT = 42;

    // default date format
    private static final String DATE_FORMAT = "MMM yyyy";

    // date format
    private String dateFormat;

    // current displayed month
    private Calendar currentDate = Calendar.getInstance();

    //event handling
    private EventHandler eventHandler = null;

    // internal components
    private LinearLayout header;
    private ImageView btnPrev;
    private ImageView btnNext;
    private TextView txtDate;
    private GridView grid;
    private ImageButton generate;

    //
    private String hostURL;
    private Date today;
    private String userName;
    private HashSet<Schedule> currentMonth = new HashSet<>();
    private String cookie;
    private Boolean isAdmin=false;

    public CalendarView(Context context) {
        super(context);
    }

    public CalendarView(Context context, AttributeSet attrs) {
        super(context, attrs);
        initControl(context, attrs);
    }

    public CalendarView(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        initControl(context, attrs);
    }

    /**
     * Load control xml layout
     */
    private void initControl(Context context, AttributeSet attrs) {
        LayoutInflater inflater = (LayoutInflater) context.getSystemService(LAYOUT_INFLATER_SERVICE);
        inflater.inflate(R.layout.control_calendar, this);

        loadDateFormat(attrs);
        assignUiElements();
        assignClickHandlers();

        updateCalendar();
    }

    @Override
    public boolean isInEditMode() {
        return super.isInEditMode();
    }

    //loading date format(string)
    private void loadDateFormat(AttributeSet attrs) {
        TypedArray ta = getContext().obtainStyledAttributes(attrs, R.styleable.CalendarView);

        try {
            // try to load provided date format, and fallback to default otherwise
            dateFormat = ta.getString(R.styleable.CalendarView_dateFormat);
            if (dateFormat == null)
                dateFormat = DATE_FORMAT;
        } finally {
            ta.recycle();
        }
    }

    //user interface buttons and grid assignment
    private void assignUiElements() {
        // layout is inflated, assign local variables to components
        header = (LinearLayout) findViewById(R.id.calendar_header);
        btnPrev = (ImageView) findViewById(R.id.calendar_prev_button);
        btnNext = (ImageView) findViewById(R.id.calendar_next_button);
        txtDate = (TextView) findViewById(R.id.calendar_date_display);
        grid = (GridView) findViewById(R.id.calendar_grid);
    }

    private void assignClickHandlers() {
        // add one month and refresh UI
        btnNext.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                currentDate.add(Calendar.MONTH, 1);
                today.setTime(currentDate.getTimeInMillis());

                try {
                    currentMonth = new HttpHandler().monthDates(hostURL + "/api/Calendar", cookie, currentDate.get(Calendar.MONTH) + 1, currentDate.get(Calendar.YEAR));
                } catch (ExecutionException | InterruptedException e) {
                    e.printStackTrace();
                }
                updateCalendar(currentMonth);

            }
        });

        // subtract one month and refresh UI
        btnPrev.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                currentDate.add(Calendar.MONTH, -1);
                today.setTime(currentDate.getTimeInMillis());

                try {
                    currentMonth = new HttpHandler().monthDates(hostURL + "/api/Calendar", cookie, currentDate.get(Calendar.MONTH) + 1, currentDate.get(Calendar.YEAR));
                } catch (ExecutionException | InterruptedException e) {
                    e.printStackTrace();
                }
                updateCalendar(currentMonth);
            }
        });


        // long-pressing a day
        grid.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> view, View cell, int position, long id) {// show returned day
                LayoutInflater inflater = (LayoutInflater) getContext().getSystemService(LAYOUT_INFLATER_SERVICE);

                Date clicked = (Date) view.getItemAtPosition(position);
                Calendar clickedCal = Calendar.getInstance();
                clickedCal.setTime(clicked);
                int day = clickedCal.get(Calendar.DAY_OF_MONTH);
                int dayOfYear = clickedCal.get(Calendar.DAY_OF_YEAR);
                int month = clickedCal.get(Calendar.MONTH);
                int year = clickedCal.get(Calendar.YEAR);

                // Inflate the custom layout/view
                View eventView = inflater.inflate(R.layout.single_event, null);
                TextView dateView = (TextView) eventView.findViewById(R.id.date);
                dateView.setText(day + "." + (month + 1) + "." + year + ".");
                //TODO uredi prikaz single eventa
                for (final Schedule eventDate : currentMonth) {
                    Calendar eventCal = Calendar.getInstance();
                    eventCal.setTime(eventDate.getDate());
                    int eventDay = eventCal.get(Calendar.DAY_OF_MONTH);
                    int eventMonth = eventCal.get(Calendar.MONTH);
                    int eventYear = eventCal.get(Calendar.YEAR);
                    if (eventDay == day && eventMonth == month && eventYear == year) {

                        //popuni single event polja
                        TextView dezurni = (TextView) eventView.findViewById(R.id.dezurni);
                        TextView office = (TextView) eventView.findViewById(R.id.office);
                        TextView phone = (TextView) eventView.findViewById(R.id.phone);
                        LinearLayout napomena = (LinearLayout) eventView.findViewById(R.id.napomenaLayout);
                        final Button traziZamjenu = (Button) eventView.findViewById(R.id.traziZamjenuButton);
                        final Button ponudiZamjenu = (Button) eventView.findViewById(R.id.ponudiZamjenuButton);

                        User scheduled = eventDate.getScheduled();
                        //holidays and weekends
                        if (eventDate.getType().equals("non-working") || eventDate.getType().equals("holiday")) {
                            dezurni.setText(eventDate.getName());
                            return;
                        }

                        //existant shift
                        else if (scheduled != null) {

                            dezurni.setVisibility(VISIBLE);
                            dezurni.setText("Dežurni: " + scheduled.getName() + " " + scheduled.getSurname());
                            office.setVisibility(VISIBLE);
                            office.setText("Soba: " + scheduled.getOffice());
                            phone.setVisibility(VISIBLE);
                            phone.setText("Kontakt broj: " + scheduled.getPhone());
                            //special day-text saved in strings
                            if (eventDate.getType().equals("special")) {
                                napomena.setVisibility(VISIBLE);
                            }

                            if (scheduled.getUsername().equals(userName)) {
                                traziZamjenu.setVisibility(VISIBLE);
                                //zatražena zamjena
                                if (eventDate.getReplaceable()) {
                                    //nema ponuda
                                    if (eventDate.getReplacementRequests().isEmpty()) {
                                        traziZamjenu.setText("Zahtjev za zamjenom poslan");
                                        traziZamjenu.setClickable(false);
                                    }
                                    //ima ponuda
                                    else {
                                        final List<Replacement> zamjene = eventDate.getReplacementRequests();
                                        traziZamjenu.setText("Pogledaj moguće zamjene(" + zamjene.size() + ")");
                                        //TODO pregled zamjena
                                        traziZamjenu.setOnClickListener(new OnClickListener() {
                                            @Override
                                            public void onClick(View view) {
                                                final Dialog replacementDialog = new Dialog(getContext());
                                                replacementDialog.setContentView(R.layout.requests_dialog);

                                                //popuni listview
                                                ListView ponude = (ListView) replacementDialog.findViewById(R.id.request_list);
                                                ReplacementsAdapter adapter = new ReplacementsAdapter(getContext(), zamjene, cookie, hostURL);
                                                ponude.setAdapter(adapter);
                                                replacementDialog.show();
                                                Button closeDialog = (Button) replacementDialog.findViewById(R.id.dialog_close_button);
                                                closeDialog.setOnClickListener(new OnClickListener() {
                                                    @Override
                                                    public void onClick(View view) {
                                                        replacementDialog.dismiss();
                                                    }
                                                });
                                            }
                                        });
                                    }
                                } else {
                                    //traži zamjenu
                                    final String shiftId = eventDate.getShiftId();
                                    traziZamjenu.setOnClickListener(new OnClickListener() {
                                        @Override
                                        public void onClick(View view) {
                                            try {
                                                String response = new HttpHandler().requestReplacement(hostURL, cookie, shiftId);
                                                if (!response.contains("Error:")) {
                                                    traziZamjenu.setClickable(false);
                                                    traziZamjenu.setText("Zahtjev za zamjenom poslan");
                                                }
                                            } catch (ExecutionException | InterruptedException e) {
                                                e.printStackTrace();
                                            }
                                        }
                                    });

                                }
                            }
                            //nije ulogirani korisnik i moguća je zamjena
                            else if (!scheduled.getUsername().equals(userName) && eventDate.getReplaceable()) {
                                ponudiZamjenu.setVisibility(VISIBLE);
                                //TODO ponuditi bezuvjetnu ili sa zamjenom određenog datuma
                                final String shiftId = eventDate.getShiftId();
  /*                              try {

                                    List<String> ponude = new HttpHandler().offerShifts(hostURL, cookie, shiftId);
                                    LinearLayout spinnerLayout = (LinearLayout)eventView.findViewById(R.id.spinner_layout);
                                    spinnerLayout.setVisibility(VISIBLE);
                                    Spinner offerReplacements = (Spinner)eventView.findViewById(R.id.spinner);

                                    ArrayAdapter<String> spinnerAdapter = new ArrayAdapter<>(getContext(), android.R.layout.simple_spinner_item, ponude);
                                    spinnerAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
                                    offerReplacements.setAdapter(spinnerAdapter);

*/
                                    ponudiZamjenu.setOnClickListener(new OnClickListener() {
                                        @Override
                                        public void onClick(View view) {
                                            try {

                                                new HttpHandler().offerReplacement(hostURL, cookie, shiftId, "");
                                                Toast.makeText(getContext(), "Ponuda poslana", Toast.LENGTH_LONG).show();
                                            } catch (ExecutionException | InterruptedException e) {
                                                e.printStackTrace();
                                            }
                                        }
                                    });
                            }
                        }
                        //non existant shift
                        else {

                            final Calendar prefCal = Calendar.getInstance();
                            prefCal.setTime(new Date());
                            int prefDay = prefCal.get(Calendar.DAY_OF_YEAR);
                            int prefYear = prefCal.get(Calendar.YEAR);
                            final Date prefDate = eventDate.getDate();
                            //ako je datum poslije today
                            if (year >= prefYear && dayOfYear > prefDay) {
                                //TODO preference
                                if (eventDate.getType().equals("special")) {
                                    napomena.setVisibility(VISIBLE);
                                }
                                traziZamjenu.setVisibility(VISIBLE);
                                traziZamjenu.setText("Odgovara mi dežurstvo");
                                ponudiZamjenu.setVisibility(VISIBLE);
                                ponudiZamjenu.setText("Ne odgovara mi dežurstvo");

                                if (eventDate.getIsPrefered() != null && eventDate.getIsPrefered()) {
                                    traziZamjenu.setBackgroundColor(Color.GREEN);
                                    traziZamjenu.setClickable(false);
                                }
                                //ako nije odabran
                                else {
                                    traziZamjenu.setOnClickListener(new OnClickListener() {
                                        @Override
                                        public void onClick(View view) {
                                            try {
                                                new HttpHandler().changePreference(hostURL, cookie, true, prefDate);
                                                traziZamjenu.setClickable(false);
                                                traziZamjenu.setBackgroundColor(Color.GREEN);
                                                setBackgroundResource(android.R.drawable.btn_default);
                                                ponudiZamjenu.setClickable(true);
                                            } catch (ExecutionException | InterruptedException e) {
                                                e.printStackTrace();
                                            }
                                        }
                                    });
                                }

                                if (eventDate.getIsPrefered() != null && !eventDate.getIsPrefered()) {
                                    ponudiZamjenu.setBackgroundColor(Color.RED);
                                    ponudiZamjenu.setClickable(false);
                                }
                                //ako je odabran
                                else {
                                    ponudiZamjenu.setOnClickListener(new OnClickListener() {
                                        @Override
                                        public void onClick(View view) {
                                            try {
                                                new HttpHandler().changePreference(hostURL, cookie, false, prefDate);
                                                ponudiZamjenu.setClickable(false);
                                                ponudiZamjenu.setBackgroundColor(Color.RED);
                                                traziZamjenu.setBackgroundResource(android.R.drawable.btn_default);
                                                traziZamjenu.setClickable(true);
                                            } catch (ExecutionException | InterruptedException e) {
                                                e.printStackTrace();
                                            }
                                        }
                                    });
                                }
                            }
                            //ako je prošao datum- ništa
                            else return;
                        }

                    }
                }


                final PopupWindow singleDateInfo = new PopupWindow(eventView, ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT);
                singleDateInfo.showAtLocation(getRootView(), Gravity.BOTTOM, 0, 100);
                ImageButton close = (ImageButton) eventView.findViewById(R.id.closeButton);
                close.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View view) {
                        singleDateInfo.dismiss();
                    }
                });

            }
        });

    }

    /**
     * Initial params needed for proper functionality
     */
    public void assignValues(String calURL, final String cookie, String userName, Boolean isAdmin) {
        this.hostURL = calURL;
        this.today = new Date();
        this.cookie = cookie;
        this.isAdmin = isAdmin;
        this.userName = userName;
        try {
            currentMonth = new HttpHandler().monthDates(hostURL + "/api/Calendar", cookie
                    , currentDate.get(Calendar.MONTH) + 1, currentDate.get(Calendar.YEAR));
        } catch (ExecutionException | InterruptedException e) {
            e.printStackTrace();
        }
        if(isAdmin){
            generate = (ImageButton) findViewById(R.id.generate_schedule);
            generate.setVisibility(VISIBLE);
            generate.setOnClickListener(new OnClickListener() {
                @Override
                public void onClick(View view) {
                    try {
                        new HttpHandler().createSchedule(hostURL, cookie,currentDate.getTime());
                        updateCalendar(currentMonth);
                    } catch (ExecutionException | InterruptedException e) {
                        e.printStackTrace();
                    }
                }
            });
        }


        updateCalendar(currentMonth);

    }

    /**
     * Display dates correctly in grid
     */
    public void updateCalendar() {
        updateCalendar(null);
    }

    /**
     * Display dates correctly in grid
     */
    public void updateCalendar(HashSet<Schedule> events) {
        ArrayList<Date> cells = new ArrayList<>();
        Calendar calendar = (Calendar) currentDate.clone();

        // determine the cell for current month's beginning
        calendar.set(Calendar.DAY_OF_MONTH, 0);
        int monthBeginningCell = calendar.get(Calendar.DAY_OF_WEEK) - 2;

        // move calendar backwards to the beginning of the week
        calendar.add(Calendar.DAY_OF_MONTH, -monthBeginningCell);

        // fill cells
        while (cells.size() < DAYS_COUNT) {
            cells.add(calendar.getTime());
            calendar.add(Calendar.DAY_OF_MONTH, 1);
        }

        // update grid
        grid.setAdapter(new CalendarAdapter(getContext(), cells, events));

        // update title
        SimpleDateFormat sdf = new SimpleDateFormat(dateFormat);
        txtDate.setText(sdf.format(currentDate.getTime()));
    }

    //prikaz pojedinog datuma u kalendaru
    private class CalendarAdapter extends ArrayAdapter<Date> {
        // days with events
        private HashSet<Schedule> eventDays;

        // for view inflation
        private LayoutInflater inflater;

        CalendarAdapter(Context context, ArrayList<Date> days, HashSet<Schedule> eventDays) {
            super(context, R.layout.control_calendar_day, days);
            this.eventDays = eventDays;
            inflater = LayoutInflater.from(context);
        }

        @NonNull
        @Override
        public View getView(int position, View view, @NonNull ViewGroup parent) {
            // day in question
            Date date = getItem(position);
            Calendar dayInQuestion = Calendar.getInstance();
            dayInQuestion.setTime(date);
            int day = dayInQuestion.get(Calendar.DAY_OF_MONTH);
            int month = dayInQuestion.get(Calendar.MONTH);
            int year = dayInQuestion.get(Calendar.YEAR);

            // inflate item if it does not exist yet
            if (view == null)
                view = inflater.inflate(R.layout.control_calendar_day, parent, false);

            // if this day has an event, specify event image
            view.setBackgroundResource(0);
            if (eventDays != null) {
                for (Schedule eventDate : eventDays) {
                    Calendar eventCal = Calendar.getInstance();
                    eventCal.setTime(eventDate.getDate());
                    int eventDay = eventCal.get(Calendar.DAY_OF_MONTH);
                    int eventMonth = eventCal.get(Calendar.MONTH);
                    int eventYear = eventCal.get(Calendar.YEAR);
                    if (eventDay == day && eventMonth == month && eventYear == year) {
                        // mark special days if (eventDate.getType().equals("holiday")) {

                        //vlastiti shift
                        //nema shift
                        if (eventDate.getScheduled() == null) {
                            //ima true/false vrijednost
                            if (eventDate.getIsPrefered() != null) {
                                if (eventDate.getIsPrefered()) {
                                    view.setBackgroundResource(R.drawable.favorite);
                                } else {
                                    view.setBackgroundResource(R.drawable.no);
                                }
                            }
                        } else {
                            //TODO Null exception
                            //mark taken
                            User marker = eventDate.getScheduled();
                            if (marker.getUsername().equals(userName)) {
                                view.setBackgroundResource(R.drawable.bell);
                            }
                            //tuđi termini, traže pomoć?
                            else {
                                if (eventDate.getReplaceable()) {
                                    view.setBackgroundResource(R.drawable.help);
                                }
                            }
                        }
                    }
                }
            }

            // clear styling
            ((TextView) view).setTypeface(null, Typeface.NORMAL);
            ((TextView) view).setTextColor(Color.BLACK);

            Calendar todaysCal = Calendar.getInstance();
            todaysCal.setTime(today);
            if (month != todaysCal.get(Calendar.MONTH) || year != todaysCal.get(Calendar.YEAR)) {
                // if this day is outside current month, grey it out
                ((TextView) view).setTextColor(Color.GRAY);
            }

            // set text
            ((TextView) view).setText(String.valueOf(dayInQuestion.get(Calendar.DATE)));

            return view;
        }
    }

    /**
     * Assign event handler to be passed needed events
     */
    public void setEventHandler(EventHandler eventHandler) {
        this.eventHandler = eventHandler;
    }

    /**
     * This interface defines what events to be reported to
     * the outside world
     */
    public interface EventHandler {
        void onDayLongPress(Date date);
    }
}
