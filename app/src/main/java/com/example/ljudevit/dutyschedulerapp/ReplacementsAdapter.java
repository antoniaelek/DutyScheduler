package com.example.ljudevit.dutyschedulerapp;

import android.content.Context;
import android.support.annotation.NonNull;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import java.text.SimpleDateFormat;
import java.util.List;
import java.util.concurrent.ExecutionException;

/**
 * Created by Ljudevit on 15.1.2017..
 */

public class ReplacementsAdapter extends ArrayAdapter<Replacement> {

    Context context;
    List<Replacement> replacements;
    String cookie;
    String URL;
    public ReplacementsAdapter(Context context, List<Replacement> values, String cookie, String url) {
        super(context, R.layout.single_replacement, values);
        this.context = context;
        this.replacements = values;
        this.URL = url;
        this.cookie = cookie;
    }

    @NonNull
    @Override
    public View getView(int position, View convertView, final ViewGroup parent) {
        LayoutInflater inflater = (LayoutInflater) context
                .getSystemService(Context.LAYOUT_INFLATER_SERVICE);

        View rowView = inflater.inflate(R.layout.single_replacement, parent, false);
        TextView name = (TextView) rowView.findViewById(R.id.name);
        TextView date = (TextView) rowView.findViewById(R.id.replacement_date);
        final Button accept = (Button) rowView.findViewById(R.id.accept_button);
        final Replacement zamjena = replacements.get(position);
        User user = zamjena.getUser();

        name.setText(user.getName()+" "+user.getSurname());
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd");
        if(zamjena.getDate()!=null) date.setText(formatter.format(zamjena.getDate()));
        else date.setText("bezuvjetna");
        accept.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                try {
                    new HttpHandler().acceptReplacement(URL,cookie, zamjena.getReplacementId());
                    Toast.makeText(getContext(),"Termin zamijenjen",Toast.LENGTH_LONG);
                    accept.setClickable(false);
                } catch (ExecutionException e) {
                    e.printStackTrace();
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
        });

        return rowView;
    }
}
