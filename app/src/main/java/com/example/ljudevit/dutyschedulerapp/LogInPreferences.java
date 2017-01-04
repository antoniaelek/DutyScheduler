package com.example.ljudevit.dutyschedulerapp;

import android.os.Bundle;
import android.preference.PreferenceActivity;
import android.preference.PreferenceFragment;
import android.preference.PreferenceManager;

public class LogInPreferences extends PreferenceActivity {

    private static final String LOGIN_PREFERENCE_INFO = "logInPreferences";
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        getFragmentManager().beginTransaction().replace(android.R.id.content, new MyPreferenceFragment()).commit();
    }

    public static class MyPreferenceFragment extends PreferenceFragment {
        @Override
        public void onCreate(final Bundle savedInstanceState) {
            super.onCreate(savedInstanceState);
            PreferenceManager prefMgr = getPreferenceManager();
            prefMgr.setSharedPreferencesName(LOGIN_PREFERENCE_INFO);
            addPreferencesFromResource(R.xml.login_settings);
        }
    }
}