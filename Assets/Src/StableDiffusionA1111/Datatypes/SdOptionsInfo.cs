using System.Collections.Generic;

namespace StableDiffusion
{
    [System.Serializable]
    public class SdOptionsInfo
    {
        public bool samples_save;
        public string samples_format;
        public string samples_filename_pattern;
        public bool save_images_add_number;
        public bool grid_save;
        public string grid_format;
        public bool grid_extended_filename;
        public bool grid_only_if_multiple;
        public bool grid_prevent_empty_spots;
        public float n_rows;
        public bool enable_pnginfo;
        public bool save_txt;
        public bool save_images_before_face_restoration;
        public bool save_images_before_highres_fix;
        public bool save_images_before_color_correction;
        public bool save_mask;
        public bool save_mask_composite;
        public float jpeg_quality;
        public bool webp_lossless;
        public bool export_for_4chan;
        public float img_downscale_threshold;
        public float target_side_length;
        public float img_max_size_mp;
        public bool use_original_name_batch;
        public bool use_upscaler_name_as_suffix;
        public bool save_selected_only;
        public bool save_init_img;
        public string temp_dir;
        public bool clean_temp_dir_at_start;
        public string outdir_samples;
        public string outdir_txt2img_samples;
        public string outdir_img2img_samples;
        public string outdir_extras_samples;
        public string outdir_grids;
        public string outdir_txt2img_grids;
        public string outdir_img2img_grids;
        public string outdir_save;
        public string outdir_init_images;
        public bool save_to_dirs;
        public bool grid_save_to_dirs;
        public bool use_save_to_dirs_for_ui;
        public string directories_filename_pattern;
        public float directories_max_prompt_words;
        public float ESRGAN_tile;
        public float ESRGAN_tile_overlap;
        public List<string> realesrgan_enabled_models;
        public string upscaler_for_img2img;
        public string face_restoration_model;
        public float code_former_weight;
        public bool face_restoration_unload;
        public bool show_warnings;
        public float memmon_poll_rate;
        public bool samples_log_stdout;
        public bool multiple_tqdm;
        public bool print_hypernet_extra;
        public bool list_hidden_files;
        public bool unload_models_when_training;
        public bool pin_memory;
        public bool save_optimizer_state;
        public bool save_training_settings_to_txt;
        public string dataset_filename_word_regex;
        public string dataset_filename_join_string;
        public float training_image_repeats_per_epoch;
        public float training_write_csv_every;
        public bool training_xattention_optimizations;
        public bool training_enable_tensorboard;
        public bool training_tensorboard_save_images;
        public float training_tensorboard_flush_every;
        public string sd_model_checkpoint;
        public float sd_checkpoint_cache;
        public float sd_vae_checkpoint_cache;
        public string sd_vae;
        public bool sd_vae_as_default;
        public float inpainting_mask_weight;
        public float initial_noise_multiplier;
        public bool img2img_color_correction;
        public bool img2img_fix_steps;
        public string img2img_background_color;
        public bool enable_quantization;
        public bool enable_emphasis;
        public bool enable_batch_seeds;
        public float comma_padding_backtrack;
        public float CLIP_stop_at_last_layers;
        public bool upcast_attn;
        public string randn_source;
        public string cross_attention_optimization;
        public float s_min_uncond;
        public int token_merging_ratio;
        public int token_merging_ratio_img2img;
        public int token_merging_ratio_hr;
        public bool use_old_emphasis_implementation;
        public bool use_old_karras_scheduler_sigmas;
        public bool no_dpmpp_sde_batch_determinism;
        public bool use_old_hires_fix_width_height;
        public bool dont_fix_second_order_samplers_schedule;
        public bool interrogate_keep_models_in_memory;
        public bool interrogate_return_ranks;
        public float interrogate_clip_num_beams;
        public float interrogate_clip_min_length;
        public float interrogate_clip_max_length;
        public float interrogate_clip_dict_limit;
        public List<string> interrogate_clip_skip_categories;
        public float interrogate_deepbooru_score_threshold;
        public bool deepbooru_sort_alpha;
        public bool deepbooru_use_spaces;
        public bool deepbooru_escape;
        public string deepbooru_filter_tags;
        public bool extra_networks_show_hidden_directories;
        public string extra_networks_hidden_models;
        public string extra_networks_default_view;
        public float extra_networks_default_multiplier;
        public float extra_networks_card_width;
        public float extra_networks_card_height;
        public string extra_networks_add_text_separator;
        public string ui_extra_networks_tab_reorder;
        public string sd_hypernetwork;
        public string localization;
        public string gradio_theme;
        public float img2img_editor_height;
        public bool return_grid;
        public bool return_mask;
        public bool return_mask_composite;
        public bool do_not_show_images;
        public bool send_seed;
        public bool send_size;
        public string font;
        public bool js_modal_lightbox;
        public bool js_modal_lightbox_initially_zoomed;
        public bool js_modal_lightbox_gamepad;
        public float js_modal_lightbox_gamepad_repeat;
        public bool show_progress_in_title;
        public bool samplers_in_dropdown;
        public bool dimensions_and_batch_together;
        public float keyedit_precision_attention;
        public float keyedit_precision_extra;
        public string keyedit_delimiters;
        public List<string> quicksettings_list;
        public List<string> ui_tab_order;
        public List<string> hidden_tabs;
        public string ui_reorder;
        public bool hires_fix_show_sampler;
        public bool hires_fix_show_prompts;
        public bool add_model_hash_to_info;
        public bool add_model_name_to_info;
        public bool add_version_to_infotext;
        public bool disable_weights_auto_swap;
        public bool show_progressbar;
        public bool live_previews_enable;
        public string live_previews_image_format;
        public bool show_progress_grid;
        public float show_progress_every_n_steps;
        public string show_progress_type;
        public string live_preview_content;
        public float live_preview_refresh_period;
        public List<string> hide_samplers;
        public float eta_ddim;
        public float eta_ancestral;
        public string ddim_discretize;
        public float s_churn;
        public float s_tmin;
        public float s_noise;
        public float eta_noise_seed_delta;
        public bool always_discard_next_to_last_sigma;
        public string uni_pc_variant;
        public string uni_pc_skip_type;
        public float uni_pc_order;
        public bool uni_pc_lower_order_final;
        public List<string> postprocessing_enable_in_main_ui;
        public List<string> postprocessing_operation_order;
        public float upscaling_max_images_in_cache;
        public List<string> disabled_extensions;
        public string disable_all_extensions;
        public string restore_config_state_file;
        public string sd_checkpoint_hash;
    }

}