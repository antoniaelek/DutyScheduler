package com.example.ljudevit.dutyschedulerapp;

import android.content.Context;
import android.support.annotation.NonNull;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;

import java.util.List;
import java.util.concurrent.ExecutionException;

class AccountsAdapter extends ArrayAdapter<User> {

    private Context context;
    private List<User> accounts;
    private String cookie;
    private String URL;
    User loggedInUser;

    /**
     * @param context context of view in witch you wish to implement
     * @param values  all system users
     * @param cookie  session cookie
     * @param url     main service address and port
     */
    AccountsAdapter(Context context, List<User> values, String cookie, String url, User loggedInUser) {
        super(context, R.layout.single_account, values);
        this.context = context;
        this.accounts = values;
        this.URL = url;
        this.cookie = cookie;
        this.loggedInUser = loggedInUser;
    }

    @NonNull
    @Override
    public View getView(int position, View convertView, @NonNull final ViewGroup parent) {
        LayoutInflater inflater = (LayoutInflater) context
                .getSystemService(Context.LAYOUT_INFLATER_SERVICE);

        final View rowView = inflater.inflate(R.layout.single_account, parent, false);
        TextView name = (TextView) rowView.findViewById(R.id.name);
        final ImageButton deleteAccountButton = (ImageButton) rowView.findViewById(R.id.delete_account);
        final User user = accounts.get(position);

        ImageButton adminValue = (ImageButton) rowView.findViewById(R.id.set_admin);
        name.setText(user.getName() + " " + user.getSurname());
        if (loggedInUser.getAdmin()) {
            final ImageView adminIcon = (ImageView) rowView.findViewById(R.id.is_admin);
            if (user.getAdmin()) adminIcon.setVisibility(View.VISIBLE);
            adminValue.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    try {
                        User changed = new HttpHandler().changeAdmin(URL, cookie, user.getUsername(), !user.getAdmin());
                        if (changed.getAdmin()) {
                            adminIcon.setVisibility(View.VISIBLE);
                        } else adminIcon.setVisibility(View.GONE);

                    } catch (ExecutionException | InterruptedException e) {
                        e.printStackTrace();
                    }
                }
            });

            deleteAccountButton.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    try {
                        new HttpHandler().deleteAccount(URL, cookie, user.getUsername());
                        rowView.setVisibility(View.GONE);
                    } catch (ExecutionException | InterruptedException e) {
                        e.printStackTrace();
                    }
                }
            });
        }
        return rowView;
    }
}
