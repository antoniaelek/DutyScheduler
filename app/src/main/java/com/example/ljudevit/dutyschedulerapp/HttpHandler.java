package com.example.ljudevit.dutyschedulerapp;

import android.os.AsyncTask;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.net.CookieHandler;
import java.net.CookieManager;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.ProtocolException;
import java.net.URISyntaxException;
import java.net.URL;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashSet;
import java.util.List;
import java.util.concurrent.ExecutionException;

class HttpHandler {

    private class replacement extends AsyncTask<String, String, String> {
        @Override
        protected String doInBackground(String... params) {
            switch (params[2]) {
                case "offerReplacement":
                    try {
                        //URL, cookie, type, shift, date
                        offerReplacement(params[0], params[1], params[3], params[4]);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    break;
                case "acceptReplacement":
                    try {
                        //URL, cookie, type, requestId
                        acceptReplacement(params[0], params[1], Integer.valueOf(params[3]));
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    break;
            }
            return null;
        }

        void offerReplacement(String URL, String cookie, String shiftId, String date) throws JSONException {
            JSONObject postObject = new JSONObject();
            postObject.put("shiftId", shiftId);
            if (!date.isEmpty()) postObject.put("date", date);
            postResponse(URL + "/api/Replacement", postObject.toString(), cookie);
        }

        void acceptReplacement(String URL, String cookie, Integer requestId) throws JSONException {
            JSONObject postObject = new JSONObject();
            postObject.put("requestId", requestId);
            postResponse(URL + "/api/Replacement/accept", postObject.toString(), cookie);
        }
    }

    String offerReplacement(String url, String cookie, String shiftId, String date) throws ExecutionException, InterruptedException {
        return new replacement().execute(url, cookie, "offerReplacement", shiftId, date).get();
    }

    String acceptReplacement(String url, String cookie, Integer requestID) throws ExecutionException, InterruptedException {
        return new replacement().execute(url, cookie, "acceptReplacement", requestID.toString()).get();
    }

    private class shift extends AsyncTask<String, String, String> {
        @Override
        protected String doInBackground(String... params) {
            //URL, cookie, type, shiftId
            switch (params[2]) {
                case "requestReplacement":
                    try {
                        return requestReplacement(params[0], params[1], params[3]);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    break;
                case "unfinishedShifts":
                    try {
                        //url, cookie, username
                        return unfinishedShifts(params[0], params[1], params[3]);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    break;
            }
            return null;
        }

        String requestReplacement(String URL, String cookie, String shiftId) throws JSONException {
            JSONObject postObject = new JSONObject();
            URL += "/api/Shift/" + shiftId;
            postObject.put("setReplaceable", true);
            return putResponse(URL, cookie, postObject.toString());
        }

        String unfinishedShifts(String URL, String cookie, String username) throws JSONException {
            URL += "/api/Shift/user" + username+"/undone";
            return getResponse(URL,cookie);
        }
    }

    String requestReplacement(String URL, String cookie, String shiftId) throws ExecutionException, InterruptedException {
        return new shift().execute(URL,cookie,"requestReplacement",shiftId).get();
    }

    /**
     *returns only string Dates, not Offers becouse of API implementation
     */
    private class offers extends AsyncTask<String, String, List<String>> {
        @Override
        protected List<String> doInBackground(String... params) {
            //URL, cookie, type, shiftId
                    try {
                        //url, cookie, username
                        String JSONresponse= unfinishedShifts(params[0], params[1], params[2]);
                        HashSet<Offer> responseOffers = new HashSet<>();
                        List<String> response = new ArrayList<>();
                        if(JSONresponse!=null) {
                            JSONArray parent = new JSONArray(JSONresponse);
                            for (int i = 0; i < parent.length(); i++) {
                                JSONObject child = parent.getJSONObject(i);
                                Offer singleOffer = new Offer();
                                singleOffer.setId(child.getInt("id"));
                                singleOffer.setUserName(child.getString("userId"));
                                SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd");
                                singleOffer.setDate(format.parse(child.getString("date")));
                                singleOffer.setReplaceble(child.getBoolean("isReplaceable"));
                                responseOffers.add(singleOffer);
                                response.add(child.getString("date"));
                            }
                        }
                        return response;
                    } catch (JSONException | ParseException e) {
                        e.printStackTrace();
                    }
            return null;
        }

        String unfinishedShifts(String URL, String cookie, String username) throws JSONException {
            URL += "/api/Shift/user" + username+"/undone";
            return getResponse(URL,cookie);
        }
    }

    public List<String> offerShifts(String url, String cookie, String shiftId) throws ExecutionException, InterruptedException{
         return new offers().execute(url, cookie, shiftId).get();

    }

    private class preference extends AsyncTask<String, String, String> {
        @Override
        protected String doInBackground(String... params) {
            //type,URL, cookie, isPrefered/preferenceID, date
            switch (params[0]) {
                case "changePreference":
                    try {
                        Boolean isPrefered = params[3].equals("true");
                        //URL, cookie, isPrefered, date
                        return changePreference(params[1], params[2], isPrefered, params[4]);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    break;
                case "deletePreference":
                    try {
                        //URL, cookie, preferenceID
                        return deletePreference(params[1], params[2], params[3]);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    break;
                case "deleteAccount":
                    try {
                        return deleteAccount(params[1], params[2], params[3]);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    break;
                case "algorithm":
                    return createSchedule(params[1], params[2], params[3],params[4]);
            }
            return null;
        }

        String createSchedule(String URL, String cookie, String year, String month){
            URL += "/api/Algorithm/year="+year+"&month=" +month;
            return getResponse(URL, cookie);
        }
        String deleteAccount(String URL, String cookie, String username) throws JSONException {
            URL += "/api/User/" + username;
            return deleteResponse(URL, cookie);
        }

        String changePreference(String URL, String cookie, Boolean isPrefered, String date) throws JSONException {
            JSONObject postObject = new JSONObject();
            URL += "/api/Preference";
            postObject.put("setPrefered", isPrefered);
            postObject.put("date", date);
            return putResponse(URL, cookie, postObject.toString());
        }

        String deletePreference(String URL, String cookie, String preferenceId) throws JSONException {
            URL += "/api/Preference/" + preferenceId;
            return deleteResponse(URL, cookie);
        }
    }

    String changePreference(String url, String cookie, Boolean isPrefered, Date date) throws ExecutionException, InterruptedException {
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd");
        //TODO scheduled prazan bug
        String dateString = formatter.format(date);
        return new preference().execute("changePreference", url, cookie, isPrefered.toString(), dateString).get();
    }

    private class session extends AsyncTask<String, String, User> {
        @Override
        protected User doInBackground(String... params) {
            switch (params[0]){
                case "logIn":
                    return logIn(params[1],params[2],params[3]);
                case "updateInfo":
                    try {
                        return updateInfo(params[1],params[2],params[3],params[4],params[5],params[6],params[7],params[8]);
                    } catch (IOException | ClassNotFoundException e) {
                        e.printStackTrace();
                    }
                    break;
                case "changeAdmin":
                    return changeAdmin(params[1],params[2],params[3]);
            }
            return null;
        }

        User changeAdmin (String url, String cookie, String isAdmin){
            JSONObject postObject = new JSONObject();
            try {
                postObject.put("setAdmin", isAdmin.equals("true"));
            } catch (JSONException e) {
                e.printStackTrace();
            }
            String serviceResponse = putResponse(url, cookie, postObject.toString());
            //seperate cookie and JSON
            String[] splitter = serviceResponse.split("~");
            User fetchedUser = new User();
            if (!serviceResponse.contains("Error:") && splitter.length>0) {
                try {
                    JSONObject recivedJson = new JSONObject(splitter[1]);
                    fetchedUser.setUsername(recivedJson.getString("username"));
                    fetchedUser.setName(recivedJson.getString("name"));
                    fetchedUser.setSurname(recivedJson.getString("lastName"));
                    fetchedUser.setEmail(recivedJson.getString("email"));
                    fetchedUser.setPhone(recivedJson.getString("phone"));
                    fetchedUser.setOffice(recivedJson.getString("office"));
                    fetchedUser.setAdmin(recivedJson.getBoolean("isAdmin"));
                    fetchedUser.setCookie(splitter[0]);
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }
            return fetchedUser;
        }

        User logIn(String url, String username, String password){
            JSONObject postObject = new JSONObject();
            try {
                if (username.contains("@")) {
                    postObject.put("email", username);
                } else postObject.put("username", username);
                postObject.put("password", password);
            } catch (JSONException e) {
                e.printStackTrace();
            }
            String serviceResponse = postResponse(url, postObject.toString(), "");
            //seperate cookie and JSON
            String[] splitter = serviceResponse.split("~");
            User fetchedUser = new User();
            if (!serviceResponse.contains("Error:") && splitter.length>0) {
                try {
                    JSONObject recivedJson = new JSONObject(splitter[1]);
                    fetchedUser.setUsername(recivedJson.getString("username"));
                    fetchedUser.setName(recivedJson.getString("name"));
                    fetchedUser.setSurname(recivedJson.getString("lastName"));
                    fetchedUser.setEmail(recivedJson.getString("email"));
                    fetchedUser.setPhone(recivedJson.getString("phone"));
                    fetchedUser.setOffice(recivedJson.getString("office"));
                    fetchedUser.setAdmin(recivedJson.getBoolean("isAdmin"));
                    fetchedUser.setCookie(splitter[0]);
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }
            return fetchedUser;
        }

        User updateInfo(String url, String cookie, String name, String lastName, String office, String phone, String email, String password) throws IOException, ClassNotFoundException {
            JSONObject postObject = new JSONObject();
            try {
                postObject.put("name", name );
                postObject.put("lastName", lastName );
                postObject.put("office", office);
                postObject.put("phone",  phone);
                postObject.put("email", email);
                postObject.put("password", password);
            } catch (JSONException e) {
                e.printStackTrace();
            }
            String serviceResponse = putResponse(url, cookie, postObject.toString());
            User fetchedUser = new User();
            if (!serviceResponse.contains("Error:")) {
                try {
                    JSONObject recivedJson = new JSONObject(serviceResponse);
                    fetchedUser.setUsername(recivedJson.getString("username"));
                    fetchedUser.setName(recivedJson.getString("name"));
                    fetchedUser.setSurname(recivedJson.getString("lastName"));
                    fetchedUser.setEmail(recivedJson.getString("email"));
                    fetchedUser.setPhone(recivedJson.getString("phone"));
                    fetchedUser.setOffice(recivedJson.getString("office"));
                    fetchedUser.setAdmin(recivedJson.getBoolean("isAdmin"));
                    fetchedUser.setPassword(password);
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }
            return fetchedUser;
        }
    }

    User changeAdmin(String url, String cookie, String username, Boolean isAdmin) throws ExecutionException, InterruptedException {
        return new session().execute("changeAdmin",url+"/api/User/"+username, cookie, isAdmin.toString()).get();
    }

    User logIn(String url, String username, String password) {
        String query = url + "/api/Session";
        User logIn = null;
        try {
            logIn = new session().execute("logIn",query, username, password).get();
        } catch (InterruptedException | ExecutionException e) {
            e.printStackTrace();
        }
        return logIn;
    }

    User updateInfo(String url, String cookie, User user){
        String query = url + "/api/User";
        User logIn = null;
        try {
            logIn = new session().execute("updateInfo", query, cookie, user.getName(), user.getSurname(),user.getOffice(), user.getPhone(), user.getEmail(), user.getPassword()).get();
        } catch (InterruptedException | ExecutionException e) {
            e.printStackTrace();
        }
        return logIn;
    }

    private class statistics extends AsyncTask<String, String, List<String>> {

        @Override
        protected List<String> doInBackground(String... params) {
            List<String> statistics = new ArrayList<>();
            String serviceResponse = getResponse(params[0], params[1]);
            try {
                JSONObject root = new JSONObject((serviceResponse));
                JSONArray me = root.getJSONArray("shifts");
                JSONArray byMe = root.getJSONArray("replacing");
                JSONArray forMe = root.getJSONArray("replaced");
                Integer o=0,s=0,t;
                for (int i = 0; i < me.length(); i++) {
                    JSONObject child = me.getJSONObject(i);
                    //samo type gledat
                    switch (child.getString("type")){
                        case "ordinary": o++;
                            break;
                        case "special":s++;
                            break;
                    }
                }
                t=o+s;
                statistics.add(o.toString());
                statistics.add(s.toString());
                statistics.add(t.toString());
                s=0;
                o=0;
                for (int i = 0; i < byMe.length(); i++) {
                    JSONObject child = me.getJSONObject(i);
                    //samo type gledat
                    switch (child.getString("type")){
                        case "ordinary": o++;
                            break;
                        case "special":s++;
                            break;
                    }
                }
                t=o+s;
                statistics.add(o.toString());
                statistics.add(s.toString());
                statistics.add(t.toString());
                s=0;
                o=0;
                for (int i = 0; i < forMe.length(); i++) {
                    JSONObject child = me.getJSONObject(i);
                    //samo type gledat
                    switch (child.getString("type")){
                        case "ordinary": o++;
                            break;
                        case "special":s++;
                            break;
                    }
                }
                t=o+s;
                statistics.add(o.toString());
                statistics.add(s.toString());
                statistics.add(t.toString());

            } catch (JSONException e) {
                e.printStackTrace();
            }
            return statistics;
        }
    }

    List<String> statistics(String url, String cookie) throws ExecutionException, InterruptedException {
        return new statistics().execute(url+"/api/Statistics", cookie).get();
    }

    private class users extends AsyncTask<String, String, List<User>> {

        @Override
        protected List<User> doInBackground(String... params) {
            List<User> accounts = new ArrayList<>();
            String serviceResponse = getResponse(params[0], params[1]);
            try {
                JSONArray root = new JSONArray(serviceResponse);
                for (int i = 0; i < root.length(); i++) {
                    JSONObject child = root.getJSONObject(i);
                    User user = new User();
                    user.setUsername(child.getString("username"));
                    user.setAdmin(child.getBoolean("isAdmin"));
                    user.setName(child.getString("name"));
                    user.setSurname(child.getString("lastName"));
                    accounts.add(user);
                }
            } catch (JSONException e) {
                e.printStackTrace();
            }
            return accounts;
        }
    }

    List<User> getUsers(String url, String cookie) throws ExecutionException, InterruptedException {
        return new users().execute(url+"/api/User", cookie).get();
    }

    private class monthData extends AsyncTask<String, String, HashSet<Schedule>> {

        @Override
        protected HashSet<Schedule> doInBackground(String... params) {
            HashSet<Schedule> monthsSchedule = new HashSet<>();
            String serviceResponse = getResponse(params[0], params[1]);
            try {
                JSONArray parent = new JSONArray(serviceResponse);
                for (int i = 0; i < parent.length(); i++) {
                    JSONObject child = parent.getJSONObject(i);
                    Schedule singleDate = new Schedule();

                    SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd");
                    //TODO scheduled prazan bug
                    Date date = formatter.parse(child.getString("date"));
                    singleDate.setDate(date);

                    singleDate.setWeekday(child.getString("weekday"));
                    singleDate.setType(child.getString("type"));
                    singleDate.setName(child.getString("name"));

                    //provjeriti prvo jesu možda null
                    if (!child.get("isPrefered").equals(null)) {
                        singleDate.setIsPrefered(child.getBoolean("isPrefered"));
                    }
                    if (!child.get("isReplaceable").equals(null)) {
                        singleDate.setReplaceable(child.getBoolean("isReplaceable"));
                    }
                    if (!child.get("shiftId").equals(null)) {
                        singleDate.setShiftId(child.getString("shiftId"));
                    }

                    JSONArray replacementParent = child.getJSONArray("replacementRequests");
                    List<Replacement> replacements = new ArrayList<>();
                    for (int j = 0; j < replacementParent.length(); j++) {
                        JSONObject singleReplacement = replacementParent.getJSONObject(j);
                        Replacement replacement = new Replacement();
                        replacement.setReplacementId(singleReplacement.getInt("id"));
                        replacement.setShiftId(singleReplacement.getInt("shiftId"));
                        replacement.setUserId(singleReplacement.getString("userId"));

                        if (!singleReplacement.get("date").equals(null)) {
                            Date replacementDate = formatter.parse(singleReplacement.getString("date"));
                            replacement.setDate(replacementDate);
                        }

                        JSONObject userReplacement = singleReplacement.getJSONObject("user");
                        User user = new User();
                        user.setUsername(userReplacement.getString("username"));
                        user.setName(userReplacement.getString("name"));
                        user.setSurname(userReplacement.getString("lastName"));
                        user.setEmail(userReplacement.getString("email"));
                        user.setPhone(userReplacement.getString("phone"));
                        user.setOffice(userReplacement.getString("office"));
                        user.setAdmin(userReplacement.getBoolean("isAdmin"));
                        replacement.setUser(user);

                        replacements.add(replacement);
                    }
                    singleDate.setReplacementRequests(replacements);

                    if (!child.get("scheduled").equals(null)) {
                        JSONObject scheduled = child.getJSONObject("scheduled");
                        User isScheduled = new User();
                        isScheduled.setUsername(scheduled.getString("username"));
                        isScheduled.setName(scheduled.getString("name"));
                        isScheduled.setSurname(scheduled.getString("lastName"));
                        isScheduled.setEmail(scheduled.getString("email"));
                        isScheduled.setPhone(scheduled.getString("phone"));
                        isScheduled.setOffice(scheduled.getString("office"));
                        isScheduled.setAdmin(scheduled.getBoolean("isAdmin"));
                        singleDate.setScheduled(isScheduled);
                    }

                    monthsSchedule.add(singleDate);
                }
            } catch (JSONException | ParseException e) {
                e.printStackTrace();
            }
            return monthsSchedule;
        }
    }

    String createSchedule(String url, String cookie, Date date) throws ExecutionException, InterruptedException {
        Calendar cal = Calendar.getInstance();
        cal.setTime(date);
        Integer year=cal.get(Calendar.YEAR);
        Integer month = cal.get(Calendar.MONTH)+1;
        return new preference().execute("algorithm",url,cookie,year.toString(),month.toString()).get();
    }

    String deleteAccount(String url, String cookie, String username) throws ExecutionException, InterruptedException {
        return new preference().execute("deleteAccount", url, cookie,username).get();
    }

    HashSet<Schedule> monthDates(String url, String cookie, Integer month, Integer year) throws ExecutionException, InterruptedException {
        String query = url + "/year=" + year + "&month=" + month;
        return new monthData().execute(query, cookie).get();
    }

    private String postResponse(String urlString, String body, String cookie) {
        HttpURLConnection urlConnection;
        String JSONstring = "";

        try {
            URL url = new URL(urlString);
            urlConnection = (HttpURLConnection) url.openConnection();

            CookieManager cookieManager = new CookieManager();
            CookieHandler.setDefault(cookieManager);

            if (!cookie.isEmpty())
                urlConnection.setRequestProperty("Cookie", cookie);

            urlConnection.setRequestProperty("Accept", "application/json");
            urlConnection.setRequestProperty("Content-Type", "application/json");
            urlConnection.setRequestMethod("POST");
            urlConnection.setConnectTimeout(5000);
            urlConnection.setReadTimeout(5000);
            urlConnection.setDoOutput(true);

            urlConnection.getOutputStream().write(body.getBytes("UTF-8"));


            int responseCode = urlConnection.getResponseCode();

            if (responseCode == HttpURLConnection.HTTP_OK) { //success
                BufferedReader reader = new BufferedReader(new InputStreamReader(
                        urlConnection.getInputStream()));
                String inputLine;
                StringBuilder response = new StringBuilder();

                //collect cookies
                cookieManager.put(url.toURI(), urlConnection.getHeaderFields());

                String cookieString = "";
                //provjeri jel postoje cookiesi
                if(!cookieManager.getCookieStore().getCookies().isEmpty()) {
                    cookieString = cookieManager.getCookieStore().getCookies().get(0).toString();
                }
                while ((inputLine = reader.readLine()) != null) {
                    response.append(inputLine);
                }
                reader.close();

                JSONstring = cookieString + "~" + response.toString();
            } else {
                JSONstring = "Error:" + responseCode;
            }

        } catch (IOException | URISyntaxException e) {
            e.printStackTrace();
        }
        return JSONstring;
    }

    private String getResponse(String urlString, String cookie) {
        HttpURLConnection urlConnection = null;
        BufferedReader reader = null;
        String JSONstring = "";

        try {
            URL url = new URL(urlString);
            urlConnection = (HttpURLConnection) url.openConnection();
            urlConnection.setRequestProperty("Accept", "application/json");

            if (!cookie.isEmpty()) {
                urlConnection.setRequestProperty("Cookie", cookie);
            }
            urlConnection.setConnectTimeout(5000);
            urlConnection.connect();

            InputStream in = urlConnection.getInputStream();
            reader = new BufferedReader(new InputStreamReader(in));

            int responseCode = urlConnection.getResponseCode();

            if (responseCode == HttpURLConnection.HTTP_OK) {
                String line;
                while ((line = reader.readLine()) != null) {
                    JSONstring += line;
                }
            } else JSONstring = "Error:" + responseCode;

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

    private String deleteResponse(String urlString, String cookie) {
        HttpURLConnection urlConnection = null;
        BufferedReader reader = null;
        String JSONstring = "";

        try {
            URL url = new URL(urlString);
            urlConnection = (HttpURLConnection) url.openConnection();
            urlConnection.setRequestProperty("Accept", "application/json");
            urlConnection.setRequestMethod("DELETE");

            if (!cookie.isEmpty()) {
                urlConnection.setRequestProperty("Cookie", cookie);
            }
            urlConnection.setConnectTimeout(5000);
            urlConnection.connect();

            InputStream in = urlConnection.getInputStream();
            reader = new BufferedReader(new InputStreamReader(in));

            int responseCode = urlConnection.getResponseCode();

            if (responseCode == HttpURLConnection.HTTP_OK) {
                String line;
                while ((line = reader.readLine()) != null) {
                    JSONstring += line;
                }
            } else JSONstring = "Error:" + responseCode;

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

    private String putResponse(String urlString, String cookie, String body) {
        HttpURLConnection urlConnection;
        BufferedWriter writer;
        String JSONstring = "";

        try {
            URL url = new URL(urlString);
            urlConnection = (HttpURLConnection) url.openConnection();

            if (!cookie.isEmpty())
                urlConnection.setRequestProperty("Cookie", cookie);

            urlConnection.setRequestProperty("Accept", "application/json");
            urlConnection.setRequestProperty("Content-Type", "application/json");
            urlConnection.setRequestMethod("PUT");
            urlConnection.setConnectTimeout(5000);

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
                JSONstring = "Error:" + responseCode;
            }

        } catch (IOException e) {
            e.printStackTrace();
        }
        return JSONstring;
    }
}

