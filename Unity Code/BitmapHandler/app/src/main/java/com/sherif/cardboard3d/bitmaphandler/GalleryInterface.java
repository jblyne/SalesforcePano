package com.sherif.cardboard3d.bitmaphandler;

import android.content.Context;
import android.database.Cursor;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.provider.MediaStore;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by Sherif Salem on 7/5/2015.
 * Produces a list of paths to all items in the android image gallery.
 */

public class GalleryInterface {

	private Context context;

	public GalleryInterface(Context _context) {
		context = _context;
	}

	public String[] GetGalleryImagePaths() {
		List<String> res = new ArrayList<String>();
		String[] proj = null;
		Cursor cursor = context.getContentResolver().query(MediaStore.Images.Media.EXTERNAL_CONTENT_URI,
															proj, null, null, null);

		if (cursor != null) {
			if (cursor.moveToFirst()) {
				do {
					String path = cursor.getString(cursor.getColumnIndex(MediaStore.Images.Media.DATA));
					File fi = new File(path);
					BitmapFactory.Options options = new BitmapFactory.Options();
					options.inJustDecodeBounds = true;
					BitmapFactory.decodeFile(fi.getAbsolutePath(), options);

					int imageHeight = options.outHeight;
					int imageWidth = options.outWidth;

					//Log.i("PathFinder", path);
					if ((float)imageWidth / (float)imageHeight > 2.0f) {
						res.add(path);
					}
				} while (cursor.moveToNext());
			}
			cursor.close();
		}
		String[] ret = res.toArray(new String[res.size()]);
		//Log.i("PathFinder", "" + ret.length);
		return ret;
	}

	public String[] GetGalleryVideoPaths() {
		List<String> res = new ArrayList<String>();

		String[] proj = null;
		Cursor cursor = context.getContentResolver().query(MediaStore.Video.Media.EXTERNAL_CONTENT_URI,
				proj, null, null, null);

		if (cursor != null) {
			if (cursor.moveToFirst()) {
				do {
					String path = cursor.getString(cursor.getColumnIndex(MediaStore.Video.Media.DATA));
					//File fi = new File(path);
					res.add(path);
				} while (cursor.moveToNext());
			}
			cursor.close();
		}

		return res.toArray(new String[res.size()]);
	}

	/**
	 * I didn't create this, it's all over the internet, in many similar forms.
	 * One of those forms: "http://www.androidsnippets.com/get-file-path-of-gallery-image"
	 * This version has been altered to replace the deprecated managedQuery() method, with
	 * getContentResolver().query()
	 */

	private String getrealPathFromURI(Uri _argURI) {
		String out = null;
		String[] proj = { MediaStore.Images.Media.DATA };

		Cursor cursor = context.getContentResolver().query(_argURI, proj, null, null, null);
		int index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
		cursor.moveToFirst();
		out = cursor.getString(index);
		cursor.close();

		return out;
	}
}
