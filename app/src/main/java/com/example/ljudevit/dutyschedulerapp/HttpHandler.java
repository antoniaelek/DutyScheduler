package com.example.ljudevit.dutyschedulerapp;

import android.os.AsyncTask;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.ProtocolException;
import java.net.URL;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutionException;

class HttpHandler  {

    private class session extends AsyncTask<String, String, User>{
        @Override
        protected User doInBackground(String... params) {
            JSONObject postObject = new JSONObject();
            try {
                if(params[1].contains("@")){
                    postObject.put("email", params[1]);
                }
                else postObject.put("username", params[1]);
                postObject.put("password", params[2]);
            } catch (JSONException e) {
                e.printStackTrace();
            }
            String serviceResponse = postResponse(params[0],postObject.toString());
            User fetchedUser = new User();
            if(!serviceResponse.contains("Error: ")) {
                try {
                    JSONObject recivedJson = new JSONObject(serviceResponse);
                    fetchedUser.setName(recivedJson.getString("name"));
                    fetchedUser.setSurname(recivedJson.getString("lastName"));
                    fetchedUser.setEmail(recivedJson.getString("email"));
                    fetchedUser.setPhone(recivedJson.getString("phone"));
                    fetchedUser.setOffice(recivedJson.getString("office"));
                    fetchedUser.setUsername(recivedJson.getString("username"));
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }
            return fetchedUser;
        }
    }

    User logIn(String url, String username, String password){
        String query = url+"/api/Session";
        User logIn = null;
        try {
            logIn = new session().execute(query,username,password).get();
        } catch (InterruptedException | ExecutionException e) {
            e.printStackTrace();
        }
        return logIn;
    }

    private class monthData extends AsyncTask<String, String, List<Schedule>>{

        @Override
        protected List<Schedule> doInBackground(String... params) {
            List<Schedule> monthsSchedule = new ArrayList<>();
            String serviceResponse = getResponse(params[0]);
            try {
                JSONArray parent = new JSONArray(serviceResponse);
                for (int i = 0; i < parent.length(); i++) {
                    JSONObject child = parent.getJSONObject(i);
                    Schedule singleDate = new Schedule();
                    singleDate.setName(child.getString("name"));
                    singleDate.setDate(child.getString("date"));
                    singleDate.setReplaceable(child.getBoolean("isReplaceable"));
                    singleDate.setType(child.getString("type"));
                    singleDate.setScheduled(child.getString("scheduled"));
                    monthsSchedule.add(singleDate);
                }
            } catch (JSONException e) {
                e.printStackTrace();
            }
            return monthsSchedule;
        }
    }

    List<Schedule> monthDates(String url, Integer month, Integer year) throws ExecutionException, InterruptedException {
        String query=url+"/year="+year+"&month="+month;
        return new monthData().execute(query).get();
    }

    private String postResponse(String urlString, String body) {
        HttpURLConnection urlConnection;
        BufferedWriter writer;
        String JSONstring="";

        try {
            URL url = new URL(urlString);
            urlConnection = (HttpURLConnection) url.openConnection();
            urlConnection.setRequestProperty("Accept", "application/json");
            urlConnection.setRequestProperty("Content-Type", "application/json");
            urlConnection.setRequestMethod("POST");
            urlConnection.setDoOutput(true);
            urlConnection.setDoInput(true);

            writer = new BufferedWriter(new OutputStreamWriter(urlConnection.getOutputStream()));

            writer.write(body);
            writer.flush();
            writer.close();

            int responseCode = urlConnection.getResponseCode();

            if (responseCode == HttpURLConnection.HTTP_OK) { //success
                BufferedReader reader = new BufferedReader(new InputStreamReader(
                        urlConnection.getInputStream()));
                String inputLine;
                StringBuilder response = new StringBuilder();

                while ((inputLine = reader.readLine()) != null) {
                    response.append(inputLine);
                }
                reader.close();

                JSONstring = response.toString();
            } else {
                JSONstring="Error: "+responseCode;
            }

        } catch (ProtocolException | MalformedURLException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }
        return JSONstring;
    }

    private String getResponse(String urlString) {
        HttpURLConnection urlConnection = null;
        BufferedReader reader = null;
        String JSONstring = "";

        try {
            URL url = new URL(urlString);
            urlConnection = (HttpURLConnection) url.openConnection();
            urlConnection.setRequestProperty("Accept", "application/json");
            urlConnection.connect();

            InputStream in = urlConnection.getInputStream();
            reader = new BufferedReader(new InputStreamReader(in));

            String line;
            while ((line = reader.readLine()) != null) {
                JSONstring += line;
            }

        } catch (Exception e) {
            e.printStackTrace();
            JSONstring = null;
        } finally {
            if (urlConnection != null) {
                urlConnection.disconnect();
            }
            try {
                if (reader != null) {
                    reader.close();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        return JSONstring;
    }
}