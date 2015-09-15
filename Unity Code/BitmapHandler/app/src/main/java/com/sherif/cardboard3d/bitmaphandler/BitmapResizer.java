package com.sherif.cardboard3d.bitmaphandler;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.location.Address;
import android.location.Geocoder;
import android.media.ExifInterface;
import android.util.Log;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.util.List;
import java.util.Locale;

/**
 * Created by Sherif on 29/4/2015.
 * Used to reduce the size of images so that Unity can access them at runtime.
 * Will decode an image, resize if necessary, then unity can access the image through its properties.
 */

public class BitmapResizer {

    private Context m_Context;
    private int m_height;
    private int m_width;
    private String m_Country;
    private String m_Date;
    private byte[] m_image;

    public int Height() {
        return m_height;
    }

    public int Width() {
        return m_width;
    }

    public String Country() { return m_Country; }

    public String Date() { return m_Date; }

    public byte[] GetImage() {
        return m_image;
    }

    public BitmapResizer(Context _context) {
        m_Context = _context;
        m_height = m_width;
        m_image = null;
        m_Country = "";
        m_Date = "";
    }

    public boolean DecodeSampledBitmapFromFile(String _fileName) {
        File inFile = new File(_fileName);
        Bitmap bm;
        int width, height;
        height = width = 4096;
        String logTag = "BitmapHandler";

        try {
            ExifInterface exif = new ExifInterface(_fileName);
            if (exif.getAttribute(ExifInterface.TAG_DATETIME) != null) {
                m_Date = exif.getAttribute(ExifInterface.TAG_DATETIME);
            }

            float loc[] = {0.0f, 0.0f};
            exif.getLatLong(loc);
            if (loc[0] != 0.0f && loc[1] != 0.0f) {
                m_Country = CountryFromLatLong(loc[0], loc[1]);
            }

        }
        catch (IOException e) {
            Log.e(logTag, e.getMessage());
        }

        BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = true;
        bm = BitmapFactory.decodeFile(inFile.getAbsolutePath(), options);

        if (options.outWidth > 4096 || options.outHeight > 4096) {
	        options.inSampleSize = calculateInSampleSize(options, 4096, 4096);
        }
        else {
            width = options.outWidth;
            height = options.outHeight;
            options.inSampleSize = 1;
        }
        options.inJustDecodeBounds = false;
        bm = BitmapFactory.decodeFile(_fileName, options);

        try {
            ByteArrayOutputStream out = new ByteArrayOutputStream();

            bm.compress(Bitmap.CompressFormat.JPEG, 100, out);
            m_image = out.toByteArray();
            m_width = bm.getWidth();
            m_height = bm.getHeight();
            out.flush();
            out.close();

            Log.e(logTag,"1. File decoded. Name: " + inFile.getName() + " Exif: Date: " + m_Date + " Country: " + m_Country);
        }
        catch (Exception e) {
            m_image = null;
            m_height = -1;
            m_width = -1;
	        Log.e(logTag, "Error occurred with image processing: " + e.getMessage());
            return false;
        }
        return true;
    }

    private int calculateInSampleSize(BitmapFactory.Options options,
                                             int reqWidth, int reqHeight) {
        // Raw height and width of image
        final int height = options.outHeight;
        final int width = options.outWidth;
        int inSampleSize = 1;

        if (height > reqHeight || width > reqWidth) {
            // Calculate the largest inSampleSize value that is a power of 2 and keeps both
            // height and width larger than the requested height and width.
            while (height / inSampleSize > reqHeight || width / inSampleSize > reqWidth) {
                inSampleSize *=2;
            }

        }
        return inSampleSize;
    }

    private String CountryFromLatLong(float _lat, float _long) {
        String out = "";
        Geocoder geocoder = new Geocoder(m_Context, Locale.getDefault());
        List<Address> addresses = null;

        try {
            addresses = geocoder.getFromLocation((double)_lat, (double)_long, 1);

            if (addresses != null && !addresses.isEmpty()) {
                return addresses.get(0).getCountryName();
            }
            else {
                return out;
            }
        }
        catch (IOException e) {
            Log.e("BitmapHandler", e.getMessage());
        }

        return out;
    }
}

